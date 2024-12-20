using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [GrainWithGuidKey]
    [StreamProvider]
    [ScheduledNode]
    public abstract partial class NodeGrain<TCommand> : Grain<NodeState<TCommand>>,
        IModifyHandler<TCommand>,
        IClearHandler,
        IEventObserver<NodeModifiedEvent>,
        IEventObserver<NodeExecutingEvent>,
        IEventObserver<NodeCancelledEvent>,
        IEventObserver<NodeCompletedEvent>,
        IEventObserver<NodeCleanedEvent>
        where TCommand : INodeCommand
    {
        protected readonly ILogger _logger;

        protected NodeGrain(ILogger logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
               GetType().Name, this.GetPrimaryKey(), reason);

            if ((State.CurrentState & NodeCurrentState.Wrote) == NodeCurrentState.Wrote)
            {
                State.ApplyWrote();
                await WriteStateAsync();
            }
            else if (State.CurrentState == NodeCurrentState.Cleaned)
                await ClearStateAsync();
        }

        [Rougamo<LoggingException>]
        public async Task ModifyAsync(TCommand item)
        {
            if ((State.CurrentState & NodeCurrentState.Executing) != NodeCurrentState.Executing)
            {
                if (State.Command is not null
                    && State.Command.Parent != default
                    && NotifyModifiedEventStream is not null)
                    await NotifyModifiedEventStream.OnNextAsync(new NodeModifiedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = State.Command.Parent
                    });

                State.ApplyModified(item);
            }
        }

        [Rougamo<LoggingException>]
        public async Task ClearAsync()
        {
            if (State.CurrentState != NodeCurrentState.None
                && State.CurrentState != NodeCurrentState.Cleaned)
            {
                State.ApplyCleaned();

                if (State.Command is not null
                    && State.Command.Parent != default
                    && NotifyCleanedEventStream is not null)
                    await NotifyCleanedEventStream.OnNextAsync(new NodeCleanedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = State.Command.Parent
                    });

                if (State.Children.Any()
                    && NotifyCleanedEventStream is not null)
                    await NotifyCleanedEventStream.OnNextBatchAsync(State.Children.Select(child => new NodeCleanedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = child.Child,
                        Mode = BatchMode.Parallel
                    }));
            }
        }

        public Task OnNextAsync(NodeModifiedEvent item)
        {
            if ((State.CurrentState & NodeCurrentState.Executing) != NodeCurrentState.Executing)
                State.ApplyModified(item);
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(NodeExecutingEvent item)
        {
            if ((State.CurrentState & NodeCurrentState.Executing) != NodeCurrentState.Executing)
            {
                State.ApplyExecuting();
                await InternaleExecuteAsync(item);
            }
        }

        protected abstract Task InternaleExecuteAsync(NodeExecutingEvent item);
        
        [Rougamo<LoggingException>]
        public async Task OnNextAsync(NodeCancelledEvent item)
        {
            if ((State.CurrentState & NodeCurrentState.Executing) == NodeCurrentState.Executing)
            {
                State.ApplyCancelled();
                await InternalCancelAsync();
            }
        }

        protected virtual async Task InternalCancelAsync()
        {
            if (State.Children.Any() && NotifyCancelledEventStream is not null)
                await NotifyCancelledEventStream.OnNextBatchAsync(State.Children.Select(child => new NodeCancelledEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = child.Child,
                    Mode = BatchMode.Parallel
                }));
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(NodeCompletedEvent item)
        {
            if (State.CurrentState == NodeCurrentState.Executing)
            {
                State.ApplyCompleted(item);

                if (State.Command is not null
                    && State.Command.Parent != default
                    && State.Children.All(child => child.CurrentState == NodeCurrentState.None)
                    && NotifyCompletedEventStream is not null)
                    await NotifyCompletedEventStream.OnNextAsync(new NodeCompletedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = State.Command.Parent
                    });
            }
        }

        public virtual async Task OnNextAsync(NodeCleanedEvent item)
        {
            if (State.CurrentState != NodeCurrentState.None
                && State.CurrentState != NodeCurrentState.Cleaned)
            {
                State.ApplyCleaned();

                if (State.Command is not null
                    && State.Command.Parent != default
                    && State.Command.Parent != item.PublisherId
                    && NotifyCleanedEventStream is not null)
                    await NotifyCleanedEventStream.OnNextAsync(new NodeCleanedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = State.Command.Parent
                    });

                if (State.Children.Any()
                    && State.Command is not null
                    && State.Command.Parent != default
                    && State.Command.Parent == item.PublisherId
                    && NotifyCleanedEventStream is not null)
                    await NotifyCleanedEventStream.OnNextBatchAsync(State.Children.Select(child => new NodeCleanedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = child.Child,
                        Mode = BatchMode.Parallel
                    }));
            }
        }
    }
}