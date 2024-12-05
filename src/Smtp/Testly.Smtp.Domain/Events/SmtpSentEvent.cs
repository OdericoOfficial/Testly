using Testly.Domain.Events;

namespace Testly.Smtp.Domain.Events
{
    public class SmtpSentEvent : SentEvent
    {
        public string From { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;
    
        public string Subject { get; set; } = string.Empty;
    }
}
