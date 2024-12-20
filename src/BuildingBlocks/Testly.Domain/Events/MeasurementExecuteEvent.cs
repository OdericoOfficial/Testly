using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    [Serializable]
    public sealed record MeasurementExecuteEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }

        public string UnitName { get; init; } = string.Empty;

        public Guid Root { get; init; }

        public int Sample { get; init; }

        public int BatchSize { get; init; }
    }
}
