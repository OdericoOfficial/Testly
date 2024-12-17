using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;

namespace Testly.Domain.States.Abstractions
{
    public abstract class ScheduledNodeState<TModifyCommand>
        where TModifyCommand : ModifyScheduledNodeCommand
    {
        public DateTime CompletedTime { get; protected set; }

        public TModifyCommand? Command { get; protected set; }

        public ScheduledNodeCurrentState CurrentState { get; protected set; }

        public virtual void ApplyExecute()
        {
            CompletedTime = default;
            CurrentState = ScheduledNodeCurrentState.Executing;
        }

        public virtual void ApplyModify(TModifyCommand item)
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

        public virtual void ApplyCompleted(ScheduledNodeCompletedEvent item)
        {
            CompletedTime = DateTime.UtcNow;
            CurrentState = ScheduledNodeCurrentState.Completed;
        }
    }
}
