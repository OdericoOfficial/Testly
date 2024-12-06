namespace Testly.Domain.States
{
    public class ScheduleLayerState
    {
        public Guid GroupId { get; set; }

        public List<ScheduleUnitCache> Units { get; set; } = [];

        public class ScheduleUnitCache
        {
            public bool IsFinished { get; set; }

            public Guid ScheduleId { get; set; }


        }
    }
}
