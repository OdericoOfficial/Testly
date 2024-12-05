namespace Testly.Domain.Events
{
    public class AggregateEvent
    {
        public DateTime SendingTime { get; set; }

        public DateTime ReceivedTime { get; set; }

        public int SentIndex { get; set; }

        public int ReceivedIndex { get; set; }
    }
}
