namespace Testly.Domain.Events.Abstractions
{
    public interface ISentEvent
    {
        DateTime SendingTime { get; init; }

        DateTime SentTime { get; init; }

        Guid AggregateId { get; init; }

        Guid ValidatorId { get; init; }
    }
}