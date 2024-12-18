using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record MeasurementCancelEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
