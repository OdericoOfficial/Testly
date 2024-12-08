﻿namespace Testly.Domain.States
{
    public enum AggregateProcess : byte
    {
        None,
        Running,
        Finished,
        Cancelled
    }

    public class AggregateState
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime LastPublish { get; set; }

        public int Sample { get; set; }

        public int BatchSize { get; set; }

        public int ReceivedSample { get; set; }

        public byte[] ReceivedMeasurement { get; set; } = [];

        public AggregateProcess Process { get; set; } = AggregateProcess.None;
    }
}
