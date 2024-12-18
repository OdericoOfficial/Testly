namespace Testly.Domain.Events.Abstractions
{
    public abstract record ReceivedEvent : IEvent
    {
        public Guid PublisherId { get; init; }
        
        public Guid SubscriberId { get; init; }
        
        public bool IsParallel { get; init; }
        
        public DateTime ReceivedTime { get; init; }
    }
}
