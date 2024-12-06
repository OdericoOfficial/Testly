namespace Testly.Domain.Events
{
    public class SummaryEvent
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Sample { get; set; }

        public int ReceivedSample { get; set; }

        public float Avg { get; set; }

        public float Mid { get; set; }
        
        public float Min { get; set; }

        public float Max { get; set; }

        public float Std { get; set; }

        public float Error { get; set; }

        public float TPS { get; set; }

        public float Quantile90 { get; set; }

        public float Quantile95 { get; set; }

        public float Quantile99 { get; set; }
    }
}
