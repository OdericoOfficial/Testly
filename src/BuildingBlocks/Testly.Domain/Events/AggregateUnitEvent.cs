using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record AggregateUnitEvent : IDomainEvent
    {
        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }

        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
