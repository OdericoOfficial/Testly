namespace Testly.Domain.Events.Abstractions
{
    public abstract record SentEvent : DomainEvent
    {
        public DateTime SendingTime { get; init; }

        public DateTime SentTime { get; init; }
    }
}