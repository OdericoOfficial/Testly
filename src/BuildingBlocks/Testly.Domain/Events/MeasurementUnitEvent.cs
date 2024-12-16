using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record MeasurementUnitEvent : DomainEvent
    {
        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }
    }
}
