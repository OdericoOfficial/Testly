using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SentValidatorGrain<TSentEvent> 
    {
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
                var measurementUnitStream = StreamProvider.GetStream<MeasurementUnitEvent>(State.SentEvent.PublisherId);

                await measurementUnitStream.OnNextAsync(new MeasurementUnitEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.SentEvent.SentTime,
                    PublisherId = ValidatorId,
                    SubscriberId = State.SentEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSent(TSentEvent sentEvent);
    }
}
