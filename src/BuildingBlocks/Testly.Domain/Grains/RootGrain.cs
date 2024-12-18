using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Commands;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription]
    [ImplicitSubscribeAsyncStream<NodeModifiedEvent>]
    [ImplicitSubscribeAsyncStream<NodeExecutingEvent>]
    [ImplicitSubscribeAsyncStream<NodeCancelledEvent>]
    [ImplicitSubscribeAsyncStream<NodeCompletedEvent>]
    [ImplicitSubscribeAsyncStream<NodeCleanedEvent>]
    [ImplicitSubscribeAsyncStream<ScalarEvent>]
    [ImplicitSubscribeAsyncStream<SummaryEvent>]
    internal sealed partial class RootGrain : NodeGrain<RootCommand>,
        IExecuteHandler,
        ICancelHandler
    {
        public RootGrain(ILogger<RootGrain> logger) : base(logger)
        {
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
            await base.OnDeactivateAsync(reason, cancellationToken);
            await UnsubscribeAllAsync();
        }

        [Rougamo<LoggingException>]
        public async Task ExecuteAsync()
        {
            if ((State.CurrentState & NodeCurrentState.Executing) != NodeCurrentState.Executing)
            {
                if (State.Children.Any() && NotifyExecutingEventStream is not null)
                    await NotifyExecutingEventStream.OnNextBatchAsync(State.Children.Select(child => new NodeExecutingEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = child.Child,
                        Mode = BatchMode.Parallel
                    }));
                State.ApplyExecuting();
            }
        }

        [Rougamo<LoggingException>]
        public async Task CancelAsync()
        {
            if ((State.CurrentState & NodeCurrentState.Executing) == NodeCurrentState.Executing)
            {
                if (State.Children.Count > 0
                    && NotifyCancelledEventStream is not null)
                    await NotifyCancelledEventStream.OnNextBatchAsync(State.Children.Select(child => new NodeCancelledEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = child.Child,
                        Mode = BatchMode.Parallel
                    }));

                State.ApplyCancelled();
            }
        }

        protected override Task InternaleExecuteAsync(NodeExecutingEvent item)
            => Task.CompletedTask;
    }
}
