namespace Testly.Domain.Events
{
    public class AggregateEvent
    {
        public DateTime SentTime { get; set; }

        public DateTime ReceivedTime { get; set; }

        public int SentIndex { get; set; }

        public int ReceivedIndex { get; set; }
    }
}
