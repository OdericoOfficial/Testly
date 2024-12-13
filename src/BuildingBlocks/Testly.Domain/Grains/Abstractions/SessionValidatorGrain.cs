using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent> 
    {
        public override async Task OnNextAsync(TSentEvent item)
        {
            switch (State.Process)
            {
                case SessionProcess.None:
                    State.ApplyNoneToContainSentEvent(item);
                    break;
                case SessionProcess.ContainReceivedEvent:
                    State.ApplyContainReceivedEventToBothContained(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        public async Task OnNextAsync(TReceivedEvent item)
        {
            switch (State.Process)
            {
                case SessionProcess.None:
                    State.ApplyNoneToContainReceivedEvent(item);
                    break;
                case SessionProcess.ContainSentEvent:
                    State.ApplyContainSentEventToBothContained(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        protected override async Task OnCompletedAsync()
        {
            if (_streamProvider != null
                && State.Process == SessionProcess.BothContained
                && ValidateSession(State.SentEvent, State.ReceivedEvent))
            {
                var aggregateStream = _streamProvider.GetStream<AggregateUnitEvent>(Constants.DefaultAggregateUnitNamespace, 
                    State.ReceivedEvent.PublisherId);

                await aggregateStream.OnNextAsync(new AggregateUnitEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.ReceivedEvent.ReceivedTime,
                    PublisherId = this.GetPrimaryKey(),
                    SubscriberId = State.ReceivedEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
