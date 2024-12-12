using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record struct StorageCompletedEvent : IDomainEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }

        public DateTime CompletedTime { get; init; }
    }
}
