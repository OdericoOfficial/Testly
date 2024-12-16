using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduledUnitGrain<TSentEvent, TRequest, TCommand>
    {
        public override async Task HandleAsync(TCommand item)
        {
            await base.HandleAsync(item);

        }

        public override async Task OnNextAsync(ScheduledNodeExecuteEvent item)
        {
            await base.OnNextAsync(item);

            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
            {
                //var measurementUnitGrain = GrainFactory.GetGrain<IMeasurementUnitGrain>(NodeId);
                //await measurementUnitGrain.ExecuteAsync(State.Command!.Sample, State.Command.BatchSize);
                
                await InternalScheduleAsync(State.Command, ExecuteSessionAsync);
            }
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Func<TCommand, Task> sessionTask);

        private async Task ExecuteSessionAsync(TCommand command)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                var request = _sessionFactory.Create(command, NodeId);

                var tuple = await _sessionFactory.CreateAsyncInvoker()
                    .Invoke(command, request);
                
                var sentEvent = await _sentFactory.CreateAsync(request, tuple, NodeId);
                var sentStream = StreamProvider.GetStream<TSentEvent>(sentEvent.SubscriberId);
                await sentStream.OnNextAsync(sentEvent);

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

        public override async Task OnNextAsync(ScheduledNodeCancelEvent item)
        {
            await base.OnNextAsync(item);

            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
                await MeasurementUnitCancelStream.OnNextAsync(new MeasurementUnitCancelEvent
                {
                    PublisherId = NodeId,
                    SubscriberId = NodeId
                });
        }
    }
}
