﻿using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Events
{
    public record NodeCompletedEvent : IEvent
    {
        public Guid PublisherId { get; init; }

        public Guid SubscriberId { get; init; }
    }
}
