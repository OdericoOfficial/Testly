namespace Testly.Domain.Events.Abstractions
{
    public interface IReceivedEvent
    {
        DateTime ReceivedTime { get; init; }

        Guid AggregateId { get; init; }

        int ReceivedIndex { get; init; }

        Guid ValidatorId { get; init; }
    }
}
