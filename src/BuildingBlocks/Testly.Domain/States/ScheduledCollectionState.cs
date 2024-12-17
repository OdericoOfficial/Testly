using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public class ScheduledCollectionState<TModifyCommand> : ScheduledNodeState<TModifyCommand>
        where TModifyCommand : ModifyScheduledNodeCommand
    {
        private class ScheduledCollectionChildState
        {
            public ScheduledNodeCurrentState CurrentState { get; set; }

            public Guid GrainId { get; set; }
        }

        private List<ScheduledCollectionChildState> InternalChilds { get; set; } = new List<ScheduledCollectionChildState>();

        public IEnumerable<Guid> Childs
            => InternalChilds.Select(child => child.GrainId);

        public override void ApplyCompleted(ScheduledNodeCompletedEvent item)
        {
            var child = InternalChilds.FirstOrDefault(temp => temp.GrainId == item.PublisherId);
            if (child is not null)
                child.CurrentState = ScheduledNodeCurrentState.Completed;
            if (InternalChilds.All(temp => temp.CurrentState == ScheduledNodeCurrentState.Completed))
            {
                CompletedTime = DateTime.UtcNow;
                CurrentState = ScheduledNodeCurrentState.Completed;
            }
        }

        public override void ApplyCancelled()
        {
            foreach (var child in InternalChilds)
                child.CurrentState = ScheduledNodeCurrentState.Cancelled;
            base.ApplyCancelled();
        }

        public void ApplyChildCleaned(ScheduledNodeCleanedEvent item)
        {
            var child = InternalChilds.FirstOrDefault(temp => temp.GrainId == item.PublisherId);
            if (child is not null)
                InternalChilds.Remove(child);
        }

        public void ApplyChildModified(ScheduledNodeModifiedEvent item)
        {
            var child = InternalChilds.FirstOrDefault(temp => temp.GrainId == item.PublisherId);
            if (child is null)
                InternalChilds.Add(new ScheduledCollectionChildState
                {
                    CurrentState = ScheduledNodeCurrentState.None,
                    GrainId = item.PublisherId
                });
        }
    }
}
