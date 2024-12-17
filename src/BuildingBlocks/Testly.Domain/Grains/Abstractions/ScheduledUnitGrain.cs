using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [ImplicitAsyncStream<MeasurementUnitExecuteEvent>]
    [ImplicitAsyncStream<MeasurementUnitCancelEvent>]
    public abstract partial class ScheduledUnitGrain<TSentEvent, TRequest, TModifyCommand> : ScheduledNodeGrain<TModifyCommand, ScheduledUnitState<TModifyCommand>> 
        where TSentEvent : SentEvent
        where TModifyCommand : ModifyScheduledUnitCommand
    {
        private readonly IScheduleSessionFactory<TRequest, TModifyCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _sentFactory;
        
        protected ScheduledUnitGrain(ILogger logger,
            IScheduleSessionFactory<TRequest, TModifyCommand> sessionFactory, 
            ISchduleSentEventFactory<TSentEvent, TRequest> sentFactory) 
                : base(logger)
        {
            _sessionFactory = sessionFactory;
            _sentFactory = sentFactory;
        }

        public override async Task OnNextAsync(ScheduledNodeExecuteEvent item)
        {
            await base.OnNextAsync(item);

            if (State.CurrentState != ScheduledNodeCurrentState.Executing
                && State.Command is not null)
            {
                await MeasurementUnitExecuteEventStream.OnNextAsync(new MeasurementUnitExecuteEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = GrainId,
                    UnitName = GetType().Name,
                    BatchSize = State.Command.BatchSize,
                    Sample = State.Command.Sample
                });
                await InternalScheduleAsync(ExecuteSessionAsync);
            }
        }

        protected abstract Task InternalScheduleAsync(Func<Task> sessionTask);

        private async Task ExecuteSessionAsync()
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                var request = _sessionFactory.Create(State.Command!, GrainId);

                var tuple = await _sessionFactory.CreateAsyncInvoker()
                    .Invoke(State.Command!, request);

                var sentEvent = await _sentFactory.CreateAsync(request, tuple, GrainId);
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
                await MeasurementUnitCancelEventStream.OnNextAsync(new MeasurementUnitCancelEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = GrainId
                });
        }
    }
}
