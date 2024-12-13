using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record ScheduledNodeExecuteEvent : IDomainEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
