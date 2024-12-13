using System.Buffers;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;
using TorchSharp;

namespace Testly.Domain.States
{
    public class AggregateUnitState
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public DateTime LastPublish { get; private set; }

        public ScheduledNodeState CurrentState { get; private set; } = ScheduledNodeState.None;

        public int Sample { get; private set; }

        public int BatchSize { get; private set; }

        public int ReceivedSample { get; private set; }

        public byte[]? ReceivedMeasurement { get; private set; } 

        public void ApplyExecute(int sample, int batchSize)
        {
            StartTime = default;
            EndTime = default;
            LastPublish = default;
            CurrentState = ScheduledNodeState.Executing;
            Sample = sample;
            BatchSize = batchSize;
            ReceivedSample = 0;
            ReceivedMeasurement = null;
        }

        public void ApplyNext(AggregateUnitEvent item)
        {
            ReceivedSample++;

            if (StartTime > item.StartTime)
                StartTime = item.StartTime;

            if (EndTime < item.EndTime)
                EndTime = item.EndTime;
                
            LastPublish = DateTime.UtcNow;
        }

        public void ApplyCompleted()
        {
            StartTime = default;
            EndTime = default;
            LastPublish = default;
            CurrentState = ScheduledNodeState.Completed;
            Sample = 0;
            BatchSize = 0;
            ReceivedSample = 0;
            ReceivedMeasurement = null;
        }

        public void ApplyCancelled()
        {
            StartTime = default;
            EndTime = default;
            LastPublish = default;
            CurrentState = ScheduledNodeState.Cancelled;
            Sample = 0;
            BatchSize = 0;
            ReceivedSample = 0;
            ReceivedMeasurement = null;
        }

        public void ApplyRent(torch.Tensor tensor)
            => ReceivedMeasurement = ArrayPool<byte>.Shared.Rent(tensor.bytes.Length);

        public void ApplyReturn()
        {
            ArrayPool<byte>.Shared.Return(ReceivedMeasurement!, true);
            ReceivedMeasurement = null;
        }
    }
}
