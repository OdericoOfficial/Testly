namespace Testly.Domain.Events
{
    public record struct AggregateEvent
    {
        public DateTime SendingTime { get; init; }

        public DateTime ReceivedTime { get; init; }

        public int ReceivedIndex { get; init; }
    }
}
