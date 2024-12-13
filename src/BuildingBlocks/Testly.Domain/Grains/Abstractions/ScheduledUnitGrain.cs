using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduledUnitGrain<TSentEvent, TRequest, TCommand>
    {
        public override async Task OnNextAsync(ScheduledNodeExecuteEvent item)
        {
            await base.OnNextAsync(item);

            if (State.CurrentState != ScheduledNodeState.Executing)
            {
                var aggregateUnitGrain = GrainFactory.GetGrain<IAggregateUnitGrain>(NodeId);
                await aggregateUnitGrain.ExecuteAsync(State.Command!.Sample, State.Command.BatchSize);
                
                await InternalScheduleAsync(State.Command, ExecuteSessionAsync);
            }
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Func<TCommand, Task> sessionTask);

        private async Task ExecuteSessionAsync(TCommand command)
        {
            if (State.CurrentState == ScheduledNodeState.Executing)
            {
                var request = _sessionFactory.Create(command, NodeId);

                var tuple = await _sessionFactory.CreateAsyncInvoker()
                    .Invoke(command, request);
                var sentEvent = await _sentFactory.CreateAsync(request, tuple, NodeId);

                var isDispose = false;

                if (request is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                    isDispose = true;
                }

                if (!isDispose && request is IDisposable disposable)
                    disposable.Dispose();

                var sentStream = StreamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, sentEvent.SubscriberId);
                await sentStream.OnNextAsync(sentEvent);
            }
        }

        public override async Task OnNextAsync(ScheduledNodeCancelEvent item)
        {
            await base.OnNextAsync(item);

            if (State.CurrentState != ScheduledNodeState.Executing)
                await AggregateUnitCancelStream.OnNextAsync(new AggregateUnitCancelEvent
                {
                    PublisherId = NodeId,
                    SubscriberId = NodeId
                });
        }
    }
}
