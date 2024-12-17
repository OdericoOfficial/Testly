using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record MeasurementUnitCompletedEvent : DomainEvent
    {
        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }
    }
}
