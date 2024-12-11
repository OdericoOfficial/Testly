namespace Testly.Domain.Events.Abstractions
{
    public interface ISentEvent : IDomainEvent
    {
        DateTime SendingTime { get; init; }

        DateTime SentTime { get; init; }
    }
}