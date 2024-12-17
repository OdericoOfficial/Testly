using System.Buffers;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;
using TorchSharp;

namespace Testly.Domain.States
{
    public class MeasurementUnitState
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public DateTime LastPublish { get; private set; }

        public ScheduledNodeCurrentState CurrentState { get; private set; } = ScheduledNodeCurrentState.None;

        public string UnitName { get; private set; } = string.Empty;

        public int Sample { get; private set; }

        public int BatchSize { get; private set; }

        public int ReceivedSample { get; private set; }

        public byte[]? ReceivedMeasurement { get; private set; } 

        public void ApplyExecute(MeasurementUnitExecuteEvent item)
        {
            StartTime = default;
            EndTime = default;
            LastPublish = default;
            CurrentState = ScheduledNodeCurrentState.Executing;
            UnitName = item.UnitName;
            Sample = item.Sample;
            BatchSize = item.Sample;
            ReceivedSample = 0;
            ReceivedMeasurement = null;
        }

        public void ApplyNext(MeasurementUnitCompletedEvent item)
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
            CurrentState = ScheduledNodeCurrentState.Completed;
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
            CurrentState = ScheduledNodeCurrentState.Cancelled;
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
