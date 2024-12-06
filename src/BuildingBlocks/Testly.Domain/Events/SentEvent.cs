namespace Testly.Domain.Events
{
    public abstract class SentEvent
    {
        public DateTime SendingTime { get; set; }
     
        public DateTime SentTime { get; set; }

        public Guid AggregateId { get; set; }

        public Guid ValidatorId { get; set; }
    }
}