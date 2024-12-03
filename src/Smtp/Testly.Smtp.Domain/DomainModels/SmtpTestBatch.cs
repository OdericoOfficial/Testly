using InfluxDB.Client.Core;

namespace Testly.Smtp.Domain.DomainModels
{
    [Measurement(nameof(SmtpTestBatch))]
    public class SmtpTestBatch
    {
        [Column(IsTimestamp = true)] public DateTime StartTime { get; set; }

        [Column(IsTimestamp = true)] public DateTime EndTime { get; set; }

        [Column(IsTag = true)] public Guid GrainId { get; set; }

        [Column(IsTag = true)] public int BatchIndex { get; set; }

        [Column] public float Min { get; set; }

        [Column] public float Max { get; set; }

        [Column] public float Mid { get; set; }

        [Column] public float Avg { get; set; }

        [Column] public float Std { get; set; }
    }
}
