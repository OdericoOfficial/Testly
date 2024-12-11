using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduleUnitGrain<TSentEvent, TRequest, TCommand> : Grain<ScheduleUnitState<TCommand>>, IScheduleUnitGrain<TCommand>
        where TSentEvent : struct, ISentEvent
        where TCommand : struct, IModifyUnitCommand
    {
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _eventFactory;

        protected readonly ILogger _logger;

        private IStreamProvider? _streamProvider;
        
        protected ScheduleUnitGrain(IScheduleSessionFactory<TRequest, TCommand> sessionFactory, ISchduleSentEventFactory<TSentEvent, TRequest> eventFactory,
            ILogger logger)
        {
            _sessionFactory = sessionFactory;
            _eventFactory = eventFactory;
            _logger = logger;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var unitId = this.GetPrimaryKey();
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task ExecuteAsync()
        {
            if (State.Process != ScheduleUnitProcess.Running)
            {
                State.ApplyExecute();

                var unitId = this.GetPrimaryKey();
                var aggregateGrain = GrainFactory.GetGrain<IAggregateUnitGrain>(unitId);
                await aggregateGrain.ExecuteAsync(State.Command.Sample, State.Command.BatchSize);

                await InternalScheduleAsync(State.Command, unitId, ExecuteSessionAsync);
            }
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Guid unitId, Func<TCommand, Guid, Task> sessionTask);

        [Rougamo<LoggingException>]
        private async Task ExecuteSessionAsync(TCommand command, Guid unitId)
        {
            if (State.Process == ScheduleUnitProcess.Running)
            {
                var request = _sessionFactory.Create(command, unitId);

                var tuple = await _sessionFactory.CreateAsyncInvoker()
                    .Invoke(command, request);
                var sentEvent = await _eventFactory.CreateAsync(request, tuple, unitId);

                var isDispose = false;

                if (request is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                    isDispose = true;
                }

                if (!isDispose && request is IDisposable disposable)
                    disposable.Dispose();

                if (_streamProvider is not null)
                {
                    var sentStream = _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, sentEvent.SubscriberId);
                    await sentStream.OnNextAsync(sentEvent);
                }
            }
        }

        public Task ModifyAsync(TCommand command)
        {
            if (State.Process != ScheduleUnitProcess.Running)
            {
                State.ApplyModify(command);
                return WriteStateAsync();
            }
            else
                return Task.CompletedTask;
        }

        public Task CancelAsync()
        {
            if (State.Process == ScheduleUnitProcess.Running)
            {
                State.ApplyCancel();
                return WriteStateAsync();
            }
            else
                return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ClearAsync()
            => State.Process != ScheduleUnitProcess.Running ? ClearStateAsync() : Task.CompletedTask;
    }
}
