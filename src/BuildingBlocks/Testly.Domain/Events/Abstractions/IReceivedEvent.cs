namespace Testly.Domain.Events.Abstractions
{
    public interface IReceivedEvent : IDomainEvent
    {
        DateTime ReceivedTime { get; init; }
    }
}
