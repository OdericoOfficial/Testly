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
        public DateTime CompletedTime { get; private set; }

        public TCommand Command { get; private set; }

        public ScheduleUnitProcess Process { get; private set; }

        public void ApplyExecute()
        {
            CompletedTime = default;
            Process = ScheduleUnitProcess.Running;
        }

        public void ApplyModify(TCommand item)
        {
            Command = item;
            CompletedTime = default;
            Process = ScheduleUnitProcess.None;
        }

        public void ApplyCancel()
        {
            CompletedTime = default;
            Process = ScheduleUnitProcess.Cancelled;
        }

        public void ApplyComplete(DateTime completedTime)
        {
            Process = ScheduleUnitProcess.Finished;
            CompletedTime = completedTime;
        }
    }
}
