namespace Testly.Domain.States
{
    public class AggregateState
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime LastPublish { get; set; }

        public int Smaple { get; set; }

        public int BatchSize { get; set; }

        public byte[] ReceivedMeasurement { get; set; } = [];

        public byte[] SentMeasurement { get; set; } = [];
    }
}
