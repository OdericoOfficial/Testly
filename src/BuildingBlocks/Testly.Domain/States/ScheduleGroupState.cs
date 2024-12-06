namespace Testly.Domain.States
{
    public class ScheduleGroupState
    {
        public DateTime ModifiedTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty; 

        public List<ScheduleLayerCache> Layers { get; set; } = [];

        public class ScheduleLayerCache
        {
            public bool IsFinished { get; set; }

            public Guid LayerId { get; set; }
        }
    }
}
