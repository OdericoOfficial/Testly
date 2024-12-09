using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class SessionValidatorGrain<TSentEvent, TReceivedEvent> : Grain<SessionState<TSentEvent, TReceivedEvent>>, ISessionValidatorGrain<TSentEvent, TReceivedEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        private readonly ILogger<SessionValidatorGrain<TSentEvent, TReceivedEvent>> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<TSentEvent>? _sentStream;
        private IAsyncStream<TReceivedEvent>? _receivedStream;
        private StreamSubscriptionHandle<TSentEvent>? _sentHandle;
        private StreamSubscriptionHandle<TReceivedEvent>? _receivedHandle;
        private int _eventCount;
        private bool _isModified;

        protected SessionValidatorGrain(ILogger<SessionValidatorGrain<TSentEvent, TReceivedEvent>> logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _isModified = false;
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            var validatorId = this.GetPrimaryKey();
            
            _sentStream = _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);
            _receivedStream = _streamProvider.GetStream<TReceivedEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);
            
            _sentHandle = await _sentStream.SubscribeAsync(OnSentEventReceivedAsync, ex =>
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    typeof(TSentEvent).Name, GetType().Name, this.GetPrimaryKey());
                return Task.CompletedTask;
            });
            _receivedHandle = await _receivedStream.SubscribeAsync(OnReceivedEventReceivedAsync, ex =>
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    typeof(TReceivedEvent).Name, GetType().Name, this.GetPrimaryKey());
                return Task.CompletedTask;
            });
            _eventCount = State.EventCount;
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (_isModified)
            {
                State.EventCount = _eventCount;
                await WriteStateAsync();
            }

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateGrain), this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public async Task OnSentEventReceivedAsync(TSentEvent sentEvent, StreamSequenceToken? sequenceToken)
        {
            _isModified = true;
            State.SentEvent = sentEvent;
            if (Interlocked.Increment(ref _eventCount) == 2)
                await PublishAsync();
        }

        [Rougamo<LoggingException>]
        public async Task OnReceivedEventReceivedAsync(TReceivedEvent receivedEvent, StreamSequenceToken? sequenceToken)
        {
            _isModified = true;
            State.ReceivedEvent = receivedEvent;
            if (Interlocked.Increment(ref _eventCount) == 2)
                await PublishAsync();
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

                await ClearStateAsync();

                if (_sentHandle is not null)
                    await _sentHandle.UnsubscribeAsync();

                if (_receivedHandle is not null)
                    await _receivedHandle.UnsubscribeAsync();
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
