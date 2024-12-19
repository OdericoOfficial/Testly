using Testly.Domain.Events.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public sealed class SessionState<TSentEvent, TReceivedEvent>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        public TSentEvent? SentEvent { get; private set; }

        public TReceivedEvent? ReceivedEvent { get; private set; }
        
        public SessionContainState ContainState { get; private set; }

        public void ApplyNoneToContainSentEvent(TSentEvent item)
        {
            SentEvent = item;
            ContainState = SessionContainState.ContainSentEvent;
        }

        public void ApplyContainReceivedEventToBothContained(TSentEvent item)
        {
            SentEvent = item;
            ContainState = SessionContainState.BothContained;
        }

        public void ApplyContainSentEventToBothContained(TReceivedEvent item)
        {
            ReceivedEvent = item;
            ContainState = SessionContainState.BothContained;
        }

        public void ApplyNoneToContainReceivedEvent(TReceivedEvent item)
        {
            ReceivedEvent = item;
            ContainState = SessionContainState.ContainReceivedEvent;
        }
    }
}
