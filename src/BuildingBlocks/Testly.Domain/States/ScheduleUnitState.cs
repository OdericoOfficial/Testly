using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.States
{
    public enum ScheduleUnitProcess : int
    {
        None = 0b_000,
        Running = 0b_001,
        Finished = 0b_010,
        Cancelled = 0b_100
    }

    public class ScheduleUnitState<TCommand>
        where TCommand : struct, IModifyUnitCommand
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public TCommand Command { get; private set; }

        public ScheduleUnitProcess Process { get; private set; }

        public void ApplyExecute()
        {
            StartTime = default;
            EndTime = default;
            Process = ScheduleUnitProcess.Running;
        }

        public void ApplyModify(TCommand item)
        {
            Command = item;
            StartTime = default;
            EndTime = default;
            Process = ScheduleUnitProcess.None;
        }

        public void ApplyCancel()
        {
            StartTime = default;
            EndTime = default;
            Process = ScheduleUnitProcess.Cancelled;
        }
    }
}
