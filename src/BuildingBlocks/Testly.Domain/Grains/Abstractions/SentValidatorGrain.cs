using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SentValidatorGrain<TSentEvent, TState> 
    {
        public virtual async Task OnNextAsync(TSentEvent item)
        {
            switch (State.Process)
            {
                case SessionProcess.None:
                    State.ApplyNoneToContainSentEvent(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        protected virtual async Task OnCompletedAsync()
        {
            if (_streamProvider != null
                && State.Process == SessionProcess.ContainSentEvent)
            {
                var aggregateStream = _streamProvider.GetStream<AggregateUnitEvent>(Constants.DefaultAggregateUnitNamespace,
                    State.SentEvent.PublisherId);

                await aggregateStream.OnNextAsync(new AggregateUnitEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.SentEvent.SentTime,
                    PublisherId = this.GetPrimaryKey(),
                    SubscriberId = State.SentEvent.PublisherId
                });
            }
        }
    }
}
