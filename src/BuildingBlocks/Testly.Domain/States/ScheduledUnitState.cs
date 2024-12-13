using Testly.Domain.Commands.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public class ScheduledUnitState<TCommand> : IScheduledNodeState<TCommand>
        where TCommand : IModifyUnitCommand
    {
        public DateTime CompletedTime { get; private set; }

        public TCommand? Command { get; private set; }

        public ScheduledNodeState CurrentState { get; private set; }

        public void ApplyExecute()
        {
            CompletedTime = default;
            CurrentState = ScheduledNodeState.Executing;
        }

        public void ApplyModify(TCommand item)
        {
            Command = item;
            CompletedTime = default;
            CurrentState = ScheduledNodeState.None;
        }

        public void ApplyCancelled()
        {
            CompletedTime = default;
            CurrentState = ScheduledNodeState.Cancelled;
        }

        public void ApplyCompleted()
        {
            CompletedTime = DateTime.UtcNow;
            CurrentState = ScheduledNodeState.Completed;
        }
    }
}
