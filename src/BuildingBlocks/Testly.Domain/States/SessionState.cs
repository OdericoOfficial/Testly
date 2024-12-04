using Testly.Domain.Events;

namespace Testly.Domain.States
{
    public class SessionState<TSentEvent, TReceivedEvent>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        public TSentEvent? SentEvent { get; set; }

        public TReceivedEvent? ReceivedEvent { get; set; }
    }
}
