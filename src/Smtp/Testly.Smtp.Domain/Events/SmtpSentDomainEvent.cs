using InfluxDB.Client.Core;

namespace Testly.Smtp.Domain.Events
{
    [Measurement(nameof(SmtpSentDomainEvent))]
    public class SmtpSentDomainEvent
    {
        [Column(IsTimestamp = true)] public DateTime SentTime { get; set; }

        [Column(IsTag = true)] public Guid GrainId { get; set; }

        [Column] public string? From { get; set; }

        [Column] public string? To { get; set; }

        [Column] public string? Subject { get; set; }

        [Column] public int SentIndex { get; set; }
    }
}
