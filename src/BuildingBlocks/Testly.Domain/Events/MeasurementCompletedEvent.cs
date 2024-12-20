using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    [Serializable]
    public sealed record MeasurementCompletedEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }

        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }
    }
}
