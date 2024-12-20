using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    [Serializable]
    public sealed record NodeCompletedEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
