using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public sealed record NodeModifiedEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
