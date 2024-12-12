﻿using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record struct ScheduleUnitCompletedEvent : IDomainEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
