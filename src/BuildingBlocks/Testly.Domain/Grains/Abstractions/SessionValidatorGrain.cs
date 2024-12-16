using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent> 
    {
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
                var measurementUnitStream = StreamProvider.GetStream<MeasurementUnitEvent>(State.ReceivedEvent.PublisherId);

                await measurementUnitStream.OnNextAsync(new MeasurementUnitEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.ReceivedEvent.ReceivedTime,
                    PublisherId = ValidatorId,
                    SubscriberId = State.ReceivedEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
