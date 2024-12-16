namespace Testly.Domain.Events.Abstractions
{
    public abstract record DomainEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
