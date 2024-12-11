using Testly.Domain.Events.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public enum SessionProcess : int
    {
        None = 0b_000,
        ContainSentEvent = 0b_001,
        ContainReceivedEvent = 0b_010,
        BothContained = 0b_100
    }

    public class SessionState<TSentEvent, TReceivedEvent> : ISessionState<TSentEvent, TReceivedEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        public TSentEvent SentEvent { get; private set; }

        public TReceivedEvent ReceivedEvent { get; private set; }
        
        public SessionProcess Process { get; private set; }

        public void ApplyContainReceivedEventToBothContained(TSentEvent item)
        {
            SentEvent = item;
            Process = SessionProcess.BothContained;
        }

        public void ApplyNoneToContainSentEvent(TSentEvent item)
        {
            SentEvent = item;
            Process = SessionProcess.ContainSentEvent;
        }

        public void ApplyContainSentEventToBothContained(TReceivedEvent item)
        {
            ReceivedEvent = item;
            Process = SessionProcess.BothContained;
        }

        public void ApplyNoneToContainReceivedEvent(TReceivedEvent item)
        {
            ReceivedEvent = item;
            Process = SessionProcess.ContainReceivedEvent;
        }
    }
}
