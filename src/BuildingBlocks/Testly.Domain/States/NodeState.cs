using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public class NodeState<TCommand>
        where TCommand : INodeCommand
    {
        public DateTime CompletedTime { get; protected set; }

        public TCommand? Command { get; protected set; }

        public List<ChildNodeState> Children { get; protected set; } = [];

        public NodeCurrentState CurrentState { get; protected set; }

        public virtual void ApplyModified(TCommand item)
        {
            Command = item;
            CompletedTime = default;
            CurrentState = NodeCurrentState.Modified | NodeCurrentState.Wrote;
        }

        public virtual void ApplyModified(NodeModifiedEvent item)
        {
            if (!Children.Any(child => child.Child == item.PublisherId))
            {
                Children.Add(new ChildNodeState
                {
                    Child = item.PublisherId
                });
                CurrentState = CurrentState | NodeCurrentState.Wrote;
            }
        }

        public virtual void ApplyExecuting()
        {
            foreach (var child in Children)
                child.CurrentState = NodeCurrentState.Executing;
            CompletedTime = default;
            CurrentState = NodeCurrentState.Executing | NodeCurrentState.Wrote;
        }

        public virtual void ApplyCompleted(NodeCompletedEvent item)
        {
            var child = Children.FirstOrDefault(temp => temp.Child == item.PublisherId);
            if (child is not null)
                child.CurrentState = NodeCurrentState.None;
            CurrentState = CurrentState | NodeCurrentState.Wrote;

            if (Children.All(child => child.CurrentState == NodeCurrentState.None))
            {
                CompletedTime = DateTime.UtcNow;
                CurrentState = NodeCurrentState.Completed | NodeCurrentState.Wrote;
            }
        }

        public virtual void ApplyCancelled()
        {
            foreach (var child in Children)
                child.CurrentState = NodeCurrentState.None;
            CompletedTime = default;
            CurrentState = NodeCurrentState.Cancelled | NodeCurrentState.Wrote;
        }

        public virtual void ApplyCleaned()
            => CurrentState = NodeCurrentState.Cleaned;

        public virtual void ApplyWrote()
            => CurrentState = CurrentState ^ NodeCurrentState.Wrote;
    }
}
