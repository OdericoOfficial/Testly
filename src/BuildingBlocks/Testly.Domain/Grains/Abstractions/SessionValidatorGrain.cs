using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class SessionValidatorGrain<TSentEvent, TReceivedEvent> : Grain<SessionState<TSentEvent, TReceivedEvent>>, ISessionValidatorGrain<TSentEvent, TReceivedEvent>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        private readonly ILogger<SessionValidatorGrain<TSentEvent, TReceivedEvent>> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<TSentEvent>? _sentStream;
        private IAsyncStream<TReceivedEvent>? _receivedStream;
        private IAsyncStream<SessionCompletedEvent>? _completedStream;
        private StreamSubscriptionHandle<TSentEvent>? _sentHandle;
        private StreamSubscriptionHandle<TReceivedEvent>? _receivedHandle;
        private StreamSubscriptionHandle<SessionCompletedEvent>? _completedHandle;

        protected SessionValidatorGrain(ILogger<SessionValidatorGrain<TSentEvent, TReceivedEvent>> logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            var validatorId = this.GetPrimaryKey();
            
            _sentStream = _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);
            _receivedStream = _streamProvider.GetStream<TReceivedEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);
            _completedStream = _streamProvider.GetStream<SessionCompletedEvent>(Constants.DefaultSessionValidatorNamespace, validatorId);

            _sentHandle = await _sentStream.SubscribeAsync(OnSentEventReceivedAsync);
            _receivedHandle = await _receivedStream.SubscribeAsync(OnReceivedEventReceivedAsync);
            _completedHandle = await _completedStream.SubscribeAsync(OnSessionCompletedAsync);
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await WriteStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateGrain), this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public async Task OnSentEventReceivedAsync(TSentEvent sentEvent, StreamSequenceToken? sequenceToken)
        {
            State.SentEvent = sentEvent;
            if (State.ReceivedEvent is not null)
                await PublishAsync();
        }

        [Rougamo<LoggingException>]
        public async Task OnReceivedEventReceivedAsync(TReceivedEvent receivedEvent, StreamSequenceToken? sequenceToken)
        {
            State.ReceivedEvent = receivedEvent;
            if (State.SentEvent is not null)
                await PublishAsync();
        }

        [Rougamo<LoggingException>]
        private async Task OnSessionCompletedAsync(SessionCompletedEvent completedEvent, StreamSequenceToken? sequenceToken)
        {
            await ClearStateAsync();

            if (_sentHandle is not null)
                await _sentHandle.UnsubscribeAsync();
        
            if (_receivedHandle is not null)
                await _receivedHandle.UnsubscribeAsync();

            if (_completedHandle is not null)
                await _completedHandle.UnsubscribeAsync();
        }

        private async Task PublishAsync()
        {
            if (ValidateSession(State.SentEvent!, State.ReceivedEvent!))
            {
                var aggregateStream = _streamProvider.GetStream<AggregateEvent>(Constants.DefaultSessionValidatorNamespace, State.ReceivedEvent!.AggregateId);
                await aggregateStream.OnNextAsync(new AggregateEvent
                {
                    SendingTime = State.SentEvent!.SendingTime,
                    ReceivedTime = State.ReceivedEvent!.ReceivedTime,
                    ReceivedIndex = State.ReceivedEvent.ReceivedIndex
                });

                await ClearStateAsync();
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
