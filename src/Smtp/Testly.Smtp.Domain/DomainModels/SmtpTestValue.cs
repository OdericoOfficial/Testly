using InfluxDB.Client.Core;

namespace Testly.Smtp.Domain.DomainModels
{
    [Measurement(nameof(SmtpTestValue))]
    public class SmtpTestValue
    {
        [Column(IsTimestamp = true)] public DateTime StartTime { get; set; }

        [Column(IsTag = true)] public DateTime EndTime { get; set; }

        [Column(IsTag = true)] public Guid GrainId { get; set; }

        [Column] public bool IsValid { get; set; }

#region SampleValue
        [Column] public int Sample { get; set; }

        [Column] public int SentCount { get; set; }

        [Column] public int ReceivedCount { get; set; }

        [Column] public int BatchSize { get; set; }

        [Column] public float Min { get; set; }

        [Column] public float Max { get; set; }

        [Column] public float Mid { get; set; }

        [Column] public float Avg { get; set; }

        [Column] public float Std { get; set; }

        [Column] public float TPS { get; set; }

        [Column] public float Los { get; set; }

        [Column] public float Quantile90 { get; set; }

        [Column] public float Quantile95 { get; set; }

        [Column] public float Quantile99 { get; set; }
#endregion    
    }
}
