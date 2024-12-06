namespace Testly.Domain.Events
{
    public abstract class ReceivedEvent
    {
        public DateTime ReceivedTime { get; set; }

        public Guid AggregateId { get; set; }
    
        public int ReceivedIndex { get; set; }

        public Guid ValidatorId { get; set; }
    }
}
