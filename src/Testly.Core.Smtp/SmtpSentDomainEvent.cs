using InfluxDB.Client.Core;

namespace Testly.Core.Smtp
{
#nullable disable
    [Measurement(nameof(SmtpReceivedDomainEvent))]
    public class SmtpSentDomainEvent
    {
        [Column] public string From { get; set; }

        [Column] public string To { get; set; }

        [Column] public string Subject { get; set; }

        [Column] public Guid GrainId { get; set; }

        [Column] public int SentIndex { get; set; }

        [Column(IsTimestamp = true)] public DateTime SentTime { get; set; }
    }
}
