using Testly.Domain.Events.Abstractions;

namespace Testly.Smtp.Domain.Events
{
    public class SmtpReceivedEvent : IReceivedEvent
    {
        public string From { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }
}
