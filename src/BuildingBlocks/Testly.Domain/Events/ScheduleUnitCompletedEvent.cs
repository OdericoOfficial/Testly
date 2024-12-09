namespace Testly.Domain.Events
{
    public record struct ScheduleUnitCompletedEvent
    {
        public DateTime StartTime { get; init; } 

        public DateTime EndTime { get; init; }

        public Guid ScheduleId { get; init; }
    }
}
