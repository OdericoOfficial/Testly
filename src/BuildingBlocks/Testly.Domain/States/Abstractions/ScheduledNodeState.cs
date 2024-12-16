using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.States.Abstractions
{
    public abstract class ScheduledNodeState<TCommand>
        where TCommand : ModifyScheduledNodeCommand
    {
        public DateTime CompletedTime { get; protected set; }

        public TCommand? Command { get; protected set; }

        public ScheduledNodeCurrentState CurrentState { get; protected set; }

        public virtual void ApplyExecute()
        {
            CompletedTime = default;
            CurrentState = ScheduledNodeCurrentState.Executing;
        }

        public virtual void ApplyModify(TCommand item)
        {
            Command = item;
            CompletedTime = default;
            CurrentState = ScheduledNodeCurrentState.None;
        }

        public virtual void ApplyCancelled()
        {
            CompletedTime = default;
            CurrentState = ScheduledNodeCurrentState.Cancelled;
        }

        public virtual void ApplyCompleted()
        {
            CompletedTime = DateTime.UtcNow;
            CurrentState = ScheduledNodeCurrentState.Completed;
        }
    }
}
