using Testly.Domain.Commands;

namespace Testly.Domain.States
{
    public class ScheduleLayerState
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public AddLayerCommand Command { get; set; }

        public int EventCount { get; set; }

        public List<ScheduleUnitCache> Units { get; set; } = [];

        public class ScheduleUnitCache
        {
            public bool IsFinished { get; set; }

            public Guid ScheduleId { get; set; }
        }
    }
}
