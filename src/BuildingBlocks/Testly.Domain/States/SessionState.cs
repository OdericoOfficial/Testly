using Testly.Domain.Events.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public class SessionState<TSentEvent, TReceivedEvent> : ISessionState<TSentEvent, TReceivedEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        public TSentEvent SentEvent { get; private set; }

        public TReceivedEvent ReceivedEvent { get; private set; }

        public byte ContainState { get; private set; }

        public void ApplyContainReceivedEventToBothContained(TSentEvent item)
        {
            SentEvent = item;
            ContainState = (byte)SessionContainState.BothContained;
        }

        public void ApplyNoneToContainSentEvent(TSentEvent item)
        {
            SentEvent = item;
            ContainState = (byte)SessionContainState.ContainSentEvent;
        }

        public void ApplyContainSentEventToBothContained(TReceivedEvent item)
        {
            ReceivedEvent = item;
            ContainState = (byte)SessionContainState.BothContained;
        }

        public void ApplyNoneToContainReceivedEvent(TReceivedEvent item)
        {
            ReceivedEvent = item;
            ContainState = (byte)SessionContainState.ContainReceivedEvent;
        }
    }
}
