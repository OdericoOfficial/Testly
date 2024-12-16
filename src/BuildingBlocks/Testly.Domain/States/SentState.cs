using Testly.Domain.Events.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public class SentState<TSentEvent>
        where TSentEvent : SentEvent
    {
        public TSentEvent? SentEvent { get; private set; }

        public SentContainState ContainState { get; private set; }

        public void ApplyNoneToContainSentEvent(TSentEvent item)
        {
            SentEvent = item;
            ContainState = SentContainState.ContainSentEvent;
        }
    }
}
