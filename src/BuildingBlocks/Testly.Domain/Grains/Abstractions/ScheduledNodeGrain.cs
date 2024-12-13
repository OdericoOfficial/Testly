using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduledNodeGrain<TCommand, TScheduledState>
    {
        public virtual Task ModifyAsync(TCommand item)
        {
            if (State.CurrentState != ScheduledNodeState.Executing)
                State.ApplyModify(item);
            return Task.CompletedTask;
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public virtual async Task ClearAsync()
        {
            if (State.CurrentState != ScheduledNodeState.Executing)
                await ClearStateAsync();
        }

        public virtual Task OnNextAsync(ScheduledNodeExecuteEvent item)
        {
            if (State.CurrentState != ScheduledNodeState.Executing)
                State.ApplyExecute();
            return Task.CompletedTask;
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public virtual async Task OnNextAsync(ScheduledNodeCompletedEvent item)
        {
            if (State.CurrentState == ScheduledNodeState.Executing)
            {
                if (LastNodeCompletedStream is not null)
                    await LastNodeCompletedStream.OnNextAsync(new ScheduledNodeCompletedEvent
                    {
                        PublisherId = NodeId,
                        SubscriberId = State.Command!.LastId
                    });

                State.ApplyCompleted();
            }
        }

        public virtual Task OnNextAsync(ScheduledNodeCancelEvent item)
        {
            if (State.CurrentState == ScheduledNodeState.Executing)
                State.ApplyCancelled();
            return Task.CompletedTask;
        }
    }
}
