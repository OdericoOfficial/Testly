using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [OverrideHandle]
    [ImplicitSubscribeAsyncStream<ScheduledNodeModifiedEvent>]
    public abstract partial class ScheduledCollectionGrain<TModifyCommand> : ScheduledNodeGrain<TModifyCommand, ScheduledCollectionState<TModifyCommand>>,
        IDomainEventAsyncObserver<ScheduledNodeModifiedEvent>
        where TModifyCommand : ModifyScheduledNodeCommand
    {
        protected ScheduledCollectionGrain(ILogger logger) : base(logger)
        {
        }

        [Rougamo<LoggingException>]
        public override async Task ClearAsync()
        {
            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
            {
                foreach (var id in State.Childs)
                {
                    var stream = StreamProvider.GetStream<ScheduledNodeCleanedEvent>(id);
                    await stream.OnNextAsync(new ScheduledNodeCleanedEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = id
                    });
                }
            }

            await base.ClearAsync();
        }

        [Rougamo<LoggingException>]
        public override async Task OnNextAsync(ScheduledNodeCancelEvent item)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                foreach (var id in State.Childs)
                {
                    var stream = StreamProvider.GetStream<ScheduledNodeCancelEvent>(id);
                    await stream.OnNextAsync(new ScheduledNodeCancelEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = id
                    });
                }
            }

            await base.OnNextAsync(item);
        }

        [Rougamo<LoggingException>]
        public override async Task OnNextAsync(ScheduledNodeCleanedEvent item)
        {
            if (State.Command is not null
                && item.PublisherId == State.Command.LastId)
                await ClearAsync();
            else if (State.Command is not null)
                State.ApplyChildCleaned(item);
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(ScheduledNodeModifiedEvent item)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
            {
                State.ApplyChildModified(item);
                await WriteStateAsync();
            }        
        }
    }
}
