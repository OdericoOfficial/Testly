using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Attributes;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [GrainWithGuidKey]
    [StreamProvider]
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent> : Grain<SessionState<TSentEvent, TReceivedEvent>>,
        IDomainEventAsyncObserver<TReceivedEvent>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        protected readonly ILogger _logger;

        [SubscribeAsyncStream]
        private IAsyncStream<TSentEvent>? _tSentEventStream;

        [SubscribeAsyncStream]
        private IAsyncStream<TReceivedEvent>? _tReceivedEventStream;
        
        protected SessionValidatorGrain(ILogger logger)
        {
            _logger = logger;   
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
            => await SubscribeAllAsync();

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await UnsubscribeAllAsync();
            await ClearStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, GrainId, reason);
        }

        public async Task OnNextAsync(TSentEvent item)
        {
            switch (State.ContainState)
            {
                case SessionContainState.None:
                    State.ApplyNoneToContainSentEvent(item);
                    break;
                case SessionContainState.ContainReceivedEvent:
                    State.ApplyContainReceivedEventToBothContained(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        public async Task OnNextAsync(TReceivedEvent item)
        {
            switch (State.ContainState)
            {
                case SessionContainState.None:
                    State.ApplyNoneToContainReceivedEvent(item);
                    break;
                case SessionContainState.ContainSentEvent:
                    State.ApplyContainSentEventToBothContained(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        private async Task OnCompletedAsync()
        {
            if (State.ContainState == SessionContainState.BothContained
                && State.SentEvent is not null
                && State.ReceivedEvent is not null
                && ValidateSession(State.SentEvent, State.ReceivedEvent))
            {
                var measurementUnitStream = StreamProvider.GetStream<MeasurementUnitCompletedEvent>(State.ReceivedEvent.PublisherId);

                await measurementUnitStream.OnNextAsync(new MeasurementUnitCompletedEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.ReceivedEvent.ReceivedTime,
                    PublisherId = GrainId,
                    SubscriberId = State.ReceivedEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}