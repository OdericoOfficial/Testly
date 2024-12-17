using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record MeasurementUnitExecuteEvent : DomainEvent
    {
        public string UnitName { get; set; } = string.Empty;

        public int Sample { get; init; }

        public int BatchSize { get; init; }
    }
}
