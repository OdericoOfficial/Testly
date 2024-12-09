using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class ScheduleUnitGrain<TSentEvent, TRequest, TCommand> : Grain<ScheduleUnitState<TCommand>>, IScheduleUnitGrain<TCommand>
        where TSentEvent : struct, ISentEvent
        where TCommand : struct, IScheduleUnitCommand
    {
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _eventFactory;
        private readonly ILogger<ScheduleUnitGrain<TSentEvent, TRequest, TCommand>> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<SummaryEvent>? _summaryStream;
        private IAsyncStream<ScalarEvent>? _scalarStream;
        private StreamSubscriptionHandle<SummaryEvent>? _summaryHandle;
        private StreamSubscriptionHandle<ScalarEvent>? _scalarHandle;
        private int _eventCount;
        private bool _isModified;

        protected ScheduleUnitGrain(IScheduleSessionFactory<TRequest, TCommand> sessionFactory, ISchduleSentEventFactory<TSentEvent, TRequest> eventFactory,
            ILogger<ScheduleUnitGrain<TSentEvent, TRequest, TCommand>> logger)
        {
            _sessionFactory = sessionFactory;
            _eventFactory = eventFactory;
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _isModified = false;
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            var scheduleId = this.GetPrimaryKey();
            _summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, scheduleId);
            _scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, scheduleId);
            _summaryHandle = await SubscribeSummaryStreamAsync(_summaryStream);
            _scalarHandle = await SubscribeScalarStreamAsync(_scalarStream);
            _eventCount = State.EventCount;
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (_isModified)
            {
                State.EventCount = _eventCount;
                await WriteStateAsync();
            }

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateGrain), this.GetPrimaryKey(), reason);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task AddUnitAsync(TCommand command)
        {
            _isModified = true;
            State.Command = command;
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Rougamo<LoggingException>]
        public Task DeleteUnitAsync()
            => ClearStateAsync();

        [Rougamo<LoggingException>]
        public async Task RunAsync()
        {
            var aggregateId = this.GetPrimaryKey();
            var aggregateGrain = GrainFactory.GetGrain<IAggregateGrain>(aggregateId);
            await aggregateGrain.StartMeasurementAsync(State.Command.Sample, State.Command.BatchSize);

            await InternalScheduleAsync(State.Command, aggregateId, RunSessionAsync);
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Guid aggregateId, Func<TCommand, Guid, Task> sessionTask);

        [Rougamo<LoggingException>]
        private async Task RunSessionAsync(TCommand command, Guid aggregateId)
        {
            var request = _sessionFactory.Create(command, aggregateId);

            var tuple = await _sessionFactory.CreateAsyncInvoker()
                .Invoke(command, request);
            var sentEvent = await _eventFactory.CreateAsync(request, tuple, aggregateId);

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
                var sentStream = _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, sentEvent.ValidatorId);
                await sentStream.OnNextAsync(sentEvent);
            }
        }

        private async Task<StreamSubscriptionHandle<SummaryEvent>> SubscribeSummaryStreamAsync(IAsyncStream<SummaryEvent> stream)
            => await stream.SubscribeAsync((summary, token) =>
            {
                _isModified = true;
                State.Summary = summary;
                return Task.CompletedTask;
            }, ex => {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    nameof(SummaryEvent), nameof(ScheduleUnitGrain<TSentEvent, TRequest, TCommand>), this.GetPrimaryKey());
                return Task.CompletedTask;
            }, [Rougamo<LoggingException>] async () => {
                if (Interlocked.Increment(ref _eventCount) == 2)
                    await OnScheduleUnitCompletedAsync();
            });

        private async Task<StreamSubscriptionHandle<ScalarEvent>> SubscribeScalarStreamAsync(IAsyncStream<ScalarEvent> stream)
            => await stream.SubscribeAsync((scalar, token) =>
            {
                _isModified = true;
                State.Scalars.Add(scalar);
                return Task.CompletedTask;
            }, ex => {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    nameof(ScalarEvent), nameof(ScheduleUnitGrain<TSentEvent, TRequest, TCommand>), this.GetPrimaryKey());
                return Task.CompletedTask;
            }, [Rougamo<LoggingException>] async () => {
                if (Interlocked.Increment(ref _eventCount) == 2)
                    await OnScheduleUnitCompletedAsync();
            });

        private async Task OnScheduleUnitCompletedAsync()
        {
            if (_summaryHandle is not null)
                await _summaryHandle.UnsubscribeAsync();

            if (_scalarHandle is not null)
                await _scalarHandle.UnsubscribeAsync();

            if (_streamProvider is not null)
            {
                var completedStream = _streamProvider.GetStream<ScheduleUnitCompletedEvent>(Constants.DefaultSchduleUnitCompletedNamespace,
                    State.Command.LayerId);
                await completedStream.OnNextAsync(new ScheduleUnitCompletedEvent
                {
                    StartTime = State.Summary.StartTime,
                    EndTime = State.Summary.EndTime,
                    ScheduleId = this.GetPrimaryKey()
                });
            }
        }
    }
}
