namespace Testly.Domain.Events
{
    public record struct SummaryEvent
    {
        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }

        public int Sample { get; init; }

        public int ReceivedSample { get; init; }

        public float Avg { get; init; }

        public float Mid { get; init; }
        
        public float Min { get; init; }

        public float Max { get; init; }

        public float Std { get; init; }

        public float Error { get; init; }

        public float TPS { get; init; }

        public float Quantile90 { get; init; }

        public float Quantile95 { get; init; }

        public float Quantile99 { get; init; }    
    }
}
