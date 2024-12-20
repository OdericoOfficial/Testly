﻿using Testly.Domain.Events.Abstractions;

namespace Testly.Smtp.Domain.Events
{
    [Serializable]
    public record SmtpReceivedEvent : ReceivedEvent
    {
        public string From { get; init; } = string.Empty;

        public string To { get; init; } = string.Empty;

        public string Subject { get; init; } = string.Empty;

        public string TextBody { get; init; } = string.Empty;
    }
}
