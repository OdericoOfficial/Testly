using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States.Abstractions;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduledNodeGrain<TCommand, TScheduledState> : Grain<TScheduledState>,
        ICommandAsyncHandler<TCommand>,
        IDomainEventAsyncObserver<ScheduledNodeExecuteEvent>,
        IDomainEventAsyncObserver<ScheduledNodeCompletedEvent>,
        IDomainEventAsyncObserver<ScheduledNodeCancelEvent>
        where TCommand : ModifyScheduledNodeCommand
        where TScheduledState : ScheduledNodeState<TCommand>
    {
        protected readonly ILogger _logger;
        
        private readonly IAsyncObserver<ScheduledNodeExecuteEvent> _nodeExecuteObserver;
        private readonly IAsyncObserver<ScheduledNodeCompletedEvent> _nodeCompletedObserver;
        private readonly IAsyncObserver<ScheduledNodeCancelEvent> _nodeCancelObserver;
        private Guid? _nodeId;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<ScheduledNodeExecuteEvent>? _nodeExecuteStream;
        private IAsyncStream<ScheduledNodeCompletedEvent>? _nodeCompletedStream;
        private IAsyncStream<ScheduledNodeCancelEvent>? _nodeCancelStream;
        private IAsyncStream<ScheduledNodeCompletedEvent>? _lastNodeCompletedStream;
        private StreamSubscriptionHandle<ScheduledNodeExecuteEvent>? _nodeExecuteHandle;
        private StreamSubscriptionHandle<ScheduledNodeCompletedEvent>? _nodeCompletedHandle;
        private StreamSubscriptionHandle<ScheduledNodeCancelEvent>? _nodeCancelHandle;

        protected Guid NodeId
            => _nodeId ??= this.GetPrimaryKey();

        protected IStreamProvider StreamProvider
            => _streamProvider ??= this.GetStreamProvider(nameof(Stream));

        private IAsyncStream<ScheduledNodeExecuteEvent> NodeExecuteStream
            => _nodeExecuteStream ??= StreamProvider.GetStream<ScheduledNodeExecuteEvent>(NodeId);

        private IAsyncStream<ScheduledNodeCompletedEvent> NodeCompletedStream
            => _nodeCompletedStream ??= StreamProvider.GetStream<ScheduledNodeCompletedEvent>(NodeId);

        private IAsyncStream<ScheduledNodeCancelEvent> NodeCancelStream
            => _nodeCancelStream ??= StreamProvider.GetStream<ScheduledNodeCancelEvent>(NodeId);

        private IAsyncStream<ScheduledNodeCompletedEvent>? LastNodeCompletedStream
            => _lastNodeCompletedStream ??=
                (State.Command?.LastId is null ? null : StreamProvider.GetStream<ScheduledNodeCompletedEvent>(State.Command.LastId));

        protected ScheduledNodeGrain(ILogger logger, 
            IAsyncObserver<ScheduledNodeExecuteEvent> nodeExecuteObserver, 
            IAsyncObserver<ScheduledNodeCompletedEvent> nodeCompletedObserver,
            IAsyncObserver<ScheduledNodeCancelEvent> nodeCancelObserver)
        {
            _logger = logger;
            _nodeExecuteObserver = nodeExecuteObserver;
            _nodeCompletedObserver = nodeCompletedObserver;
            _nodeCancelObserver = nodeCancelObserver;
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) 
                return;

            _nodeCompletedHandle = await NodeCompletedStream.SubscribeAsync(_nodeCompletedObserver);
            _nodeCancelHandle = await NodeCancelStream.SubscribeAsync(_nodeCancelObserver);
            _nodeExecuteHandle = await NodeExecuteStream.SubscribeAsync(_nodeExecuteObserver);
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.None)
                await WriteStateAsync();

            await UnsubscribeSetNullAsync(ref _nodeCompletedHandle);
            await UnsubscribeSetNullAsync(ref _nodeCancelHandle);
            await UnsubscribeSetNullAsync(ref _nodeExecuteHandle);

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }
    }
}
