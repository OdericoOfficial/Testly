using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record NodeCleanedEvent : IBatchEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }

        public BatchMode Mode { get; init; }
    }
}
