namespace Testly.Domain.Events
{
    public record struct ScheduleLayerCompletedEvent
    {
        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }

        public Guid LayerId { get; init; }
    }
}
