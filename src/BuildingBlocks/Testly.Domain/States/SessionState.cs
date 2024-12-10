using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.States
{
    public class SessionState<TSentEvent, TReceivedEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        public TSentEvent SentEvent { get; set; }

        public TReceivedEvent ReceivedEvent { get; set; }
        
        public int CompletedCount { get; set; }
    }
}
