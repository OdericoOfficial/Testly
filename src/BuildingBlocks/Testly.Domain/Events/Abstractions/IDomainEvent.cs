namespace Testly.Domain.Events.Abstractions
{
    public interface IDomainEvent
    {
        Guid PublisherId { get; init; }

        Guid SubscriberId { get; init; }
    }
}
