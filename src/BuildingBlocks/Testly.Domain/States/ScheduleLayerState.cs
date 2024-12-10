using Testly.Domain.Commands;

namespace Testly.Domain.States
{
    public class ScheduleLayerState
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public ModifyLayerCommand Command { get; set; }

        public List<ScheduleUnitCache> Units { get; set; } = [];

        public class ScheduleUnitCache
        {
            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public bool IsFinished { get; set; }

            public Guid UnitId { get; set; }
        }
    }
}
