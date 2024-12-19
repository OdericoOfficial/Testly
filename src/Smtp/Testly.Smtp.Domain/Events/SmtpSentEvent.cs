using Testly.Domain.Events.Abstractions;

namespace Testly.Smtp.Domain.Events
{
    public record SmtpSentEvent : SentEvent
    {
        public string From { get; init; } = string.Empty;

        public string To { get; init; } = string.Empty;

        public string Subject { get; init; } = string.Empty;
    
        public string TextBody { get; init; } = string.Empty;
    }
}
