using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent> : Grain<SessionState<TSentEvent, TReceivedEvent>>, ISessionValidatorGrain<TSentEvent, TReceivedEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        #region Field
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private IStreamProvider? _streamProvider;

        #region AsyncStream
        private IAsyncStream<TSentEvent>? _sentStream;
        private IAsyncStream<TReceivedEvent>? _receivedStream;
        #endregion

        #region AsyncObserver
        private IAsyncObserver<TSentEvent>? _sentObserver;
        private IAsyncObserver<TReceivedEvent>? _receivedObserver;
        #endregion

        #region SubscriptionHandle
        private StreamSubscriptionHandle<TSentEvent>? _sentHandle;
        private StreamSubscriptionHandle<TReceivedEvent>? _receivedHandle;
        #endregion

        private int _completedCount;
        #endregion

        protected SessionValidatorGrain(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            DelayDeactivation(TimeSpan.FromMinutes(2));
            _completedCount = State.CompletedCount;        
            var validatorId = this.GetPrimaryKey();
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);

            #region AsyncStream
            _sentStream = _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);
            _receivedStream = _streamProvider.GetStream<TReceivedEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);
            #endregion

            #region AsyncObserver
            _sentObserver = new SentEventObserver(this, _loggerFactory.CreateLogger<SentEventObserver>());
            _receivedObserver = new ReceivedEventObserver(this, _loggerFactory.CreateLogger<ReceivedEventObserver>());
            #endregion

            #region SubscriptionHandle
            _sentHandle = await _sentStream.SubscribeAsync(_sentObserver);
            _receivedHandle = await _receivedStream.SubscribeAsync(_receivedObserver);
            #endregion

        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _completedCount = 0;
            await ClearStateAsync();

            #region SubscriptionHandle
            if (_sentHandle is not null)
                await _sentHandle.UnsubscribeAsync();

            if (_receivedHandle is not null)
                await _receivedHandle.UnsubscribeAsync();
            #endregion

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }

        private async Task PublishAsync()
        {
            if (ValidateSession(State.SentEvent, State.ReceivedEvent))
            {
                var aggregateStream = _streamProvider.GetStream<AggregateEvent>(Constants.DefaultSessionValidatorNamespace, State.ReceivedEvent!.AggregateId);
                await aggregateStream.OnNextAsync(new AggregateEvent
                {
                    SendingTime = State.SentEvent.SendingTime,
                    ReceivedTime = State.ReceivedEvent.ReceivedTime,
                    ReceivedIndex = State.ReceivedEvent.ReceivedIndex
                });
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
