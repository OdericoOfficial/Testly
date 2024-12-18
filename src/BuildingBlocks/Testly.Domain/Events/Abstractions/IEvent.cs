namespace Testly.Domain.Events.Abstractions
{
    public interface IEvent
    {
        Guid PublisherId { get; init; }

        Guid SubscriberId { get; init; }
    }
}
