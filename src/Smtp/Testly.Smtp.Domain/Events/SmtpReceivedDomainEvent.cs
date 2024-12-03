using InfluxDB.Client.Core;

namespace Testly.Smtp.Domain.Events
{
    [Measurement(nameof(SmtpReceivedDomainEvent))]
    public class SmtpReceivedDomainEvent
    {
        [Column(IsTimestamp = true)] public DateTime ReceivedTime { get; set; }

        [Column(IsTag = true)] public Guid GrainId { get; set; }

        [Column] public string? From { get; set; }

        [Column] public string? To { get; set; }

        [Column] public string? Subject { get; set; }

        [Column] public int ReceivedIndex { get; set; }
    }
}