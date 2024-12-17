using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record ScalarResultEvent : DomainEvent
    {
        public string UnitName { get; init; } = string.Empty;

        public int Index { get; init; }

        public float Avg { get; init; }

        public float Mid { get; init; }

        public float Min { get; init; }

        public float Max { get; init; }

        public float Std { get; init; }
    }
}
