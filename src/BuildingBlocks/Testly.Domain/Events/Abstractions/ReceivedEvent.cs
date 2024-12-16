namespace Testly.Domain.Events.Abstractions
{
    public abstract record ReceivedEvent : DomainEvent
    {
        public DateTime ReceivedTime { get; init; }
    }
}
