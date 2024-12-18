namespace Testly.Domain.Events.Abstractions
{
    public interface IBatchEvent : IEvent
    {
        public BatchMode Mode { get; init; }
    }
}
