using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record MeasurementUnitModifyEvent : DomainEvent
    {
        public int Sample { get; init; }

        public int BatchSize { get; init; }
    }
}
