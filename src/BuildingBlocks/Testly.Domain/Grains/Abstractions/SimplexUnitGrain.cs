using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [ImplicitSubscribeAsyncStream<MeasurementExecuteEvent>]
    [ImplicitSubscribeAsyncStream<MeasurementCompletedEvent>]
    [ImplicitSubscribeAsyncStream<MeasurementCancelEvent>]
    public abstract partial class SimplexUnitGrain<TSentEvent, TRequest, TCommand> : NodeGrain<TCommand> 
        where TSentEvent : SentEvent
        where TCommand : IUnitCommand
    {
        private readonly ISentPolicy<TCommand> _policy;
        private readonly ISentFactory<TRequest> _sentFactory;
        private readonly ISentEventFactory<TSentEvent, TRequest> _sentEventFactory;

        [SubscribeAsyncStream]
        private IAsyncStream<TSentEvent>? _tSentEventStream;

        protected SimplexUnitGrain(ILogger logger,
            ISentPolicy<TCommand> policy,
            ISentFactory<TRequest> sentFactory, 
            ISentEventFactory<TSentEvent, TRequest> sentEventFactory) : base(logger)
        {
            _policy = policy;
            _sentFactory = sentFactory;
            _sentEventFactory = sentEventFactory;
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

        protected override async Task InternaleExecuteAsync(NodeExecutingEvent item)
        {
            if (State.Command is not null)
            {
                await MeasurementExecuteEventStream.OnNextAsync(new MeasurementExecuteEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = GrainId,
                    UnitName = GetType().Name,
                    BatchSize = State.Command.BatchSize,
                    Root = State.Command.Root,
                    Sample = State.Command.Sample
                });

                await _policy.ScheduleAsync(State.Command, ExecuteSentAsync);
            }
        }

        [Rougamo<LoggingException>]
        private async Task ExecuteSentAsync()
        {
            if (State.CurrentState == NodeCurrentState.Executing
                && State.Command is not null
                && TSentEventStream is not null)
            {
                var request = _sentFactory.Create(GrainId);

                var tuple = await _sentFactory.CreateAsyncInvoker()
                    .Invoke(request);

                var sentEvent = await _sentEventFactory.CreateAsync(request, tuple, GrainId);
                await TSentEventStream.OnNextAsync(sentEvent);

                await DisposeRequestAsync(request);
            }
        }

        private ValueTask DisposeRequestAsync(TRequest request)
        {
            if (request is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();
            else if (request is IDisposable disposable)
                disposable.Dispose();
            return ValueTask.CompletedTask;
        }

        protected override Task InternalCancelAsync()
            => MeasurementCancelEventStream.OnNextAsync(new MeasurementCancelEvent
            {
                PublisherId = GrainId,
                SubscriberId = GrainId
            });
    }
}
