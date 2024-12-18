namespace Testly.Domain.Events.Abstractions
{
    public abstract record SentEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }

        public bool IsParallel { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTime SendingTime { get; init; }

        public DateTime SentTime { get; init; }
    }
}