using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [GrainWithGuidKey]
    [StreamProvider]
    [ScheduledNode]
    [ImplicitSubscribeAsyncStream<ScheduledNodeExecuteEvent>]
    [ImplicitSubscribeAsyncStream<ScheduledNodeCancelEvent>]
    [ImplicitSubscribeAsyncStream<ScheduledNodeCompletedEvent>]
    [ImplicitSubscribeAsyncStream<ScheduledNodeCleanedEvent>]
    public abstract partial class ScheduledNodeGrain<TModifyCommand, TScheduledState> : Grain<TScheduledState>,
        IModifyCommandAsyncHandler<TModifyCommand>,
        IClearCommandAsyncHandler,
        IDomainEventAsyncObserver<ScheduledNodeExecuteEvent>,
        IDomainEventAsyncObserver<ScheduledNodeCancelEvent>,
        IDomainEventAsyncObserver<ScheduledNodeCompletedEvent>,
        IDomainEventAsyncObserver<ScheduledNodeCleanedEvent>
        where TModifyCommand : ModifyScheduledNodeCommand
        where TScheduledState : ScheduledNodeState<TModifyCommand>
    {
        protected readonly ILogger _logger;

        protected ScheduledNodeGrain(ILogger logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) 
                return;
            await SubscribeAllAsync();
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.None)
                await WriteStateAsync();

            await UnsubscribeAllAsync();
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public virtual async Task ModifyAsync(TModifyCommand item)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
            {
                var isInit = State.Command is null;
                State.ApplyModify(item);
                if (LastNodeModifiedEventStream is not null && isInit)
                    await LastNodeModifiedEventStream.OnNextAsync(new ScheduledNodeModifiedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = State.Command!.LastId
                    });
            }
        }

        [Rougamo<LoggingException>]
        public virtual async Task ClearAsync()
        {
            var lastNodeCleanedEventStream = LastNodeCleanedEventStream;
            await ClearStateAsync();
            if (lastNodeCleanedEventStream is not null)
                await lastNodeCleanedEventStream.OnNextAsync(new ScheduledNodeCleanedEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = State.Command!.LastId,
                });
        }

        public virtual Task OnNextAsync(ScheduledNodeExecuteEvent item)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
                State.ApplyExecute();
            return Task.CompletedTask;
        }

        public virtual Task OnNextAsync(ScheduledNodeCancelEvent item)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
                State.ApplyCancelled();
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(ScheduledNodeCompletedEvent item)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                State.ApplyCompleted(item);
                if (LastNodeCompletedEventStream is not null)
                    await LastNodeCompletedEventStream.OnNextAsync(new ScheduledNodeCompletedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = State.Command!.LastId
                    });
            }
        }

        public virtual Task OnNextAsync(ScheduledNodeCleanedEvent item)
        {
            if (State.Command is not null
                && item.PublisherId == State.Command.LastId)
                return ClearAsync();
            return Task.CompletedTask;
        }
    }
}
