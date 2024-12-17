using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
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
    public abstract partial class SentValidatorGrain<TSentEvent> : Grain<SentState<TSentEvent>>, 
        IDomainEventAsyncObserver<TSentEvent>,
        IRougamo<LoggingException>
        where TSentEvent : SentEvent
    {
        protected readonly ILogger _logger;

        [SubscribeAsyncStream]
        private IAsyncStream<TSentEvent>? _tSentEventStream;

        protected SentValidatorGrain(ILogger logger)
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
                case SentContainState.ContainSentEvent:
                    State.ApplyNoneToContainSentEvent(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        private async Task OnCompletedAsync()
        {
            if (State.ContainState == SentContainState.ContainSentEvent
                && State.SentEvent is not null
                && ValidateSent(State.SentEvent))
            {
                var measurementUnitStream = StreamProvider.GetStream<MeasurementUnitCompletedEvent>(State.SentEvent.PublisherId);

                await measurementUnitStream.OnNextAsync(new MeasurementUnitCompletedEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.SentEvent.SentTime,
                    PublisherId = GrainId,
                    SubscriberId = State.SentEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSent(TSentEvent sentEvent);
    }
}
