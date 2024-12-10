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
    public abstract partial class ScheduleUnitGrain<TSentEvent, TRequest, TCommand> : Grain<ScheduleUnitState<TCommand>>, IScheduleUnitGrain<TCommand>
        where TSentEvent : struct, ISentEvent
        where TCommand : struct, IModifyUnitCommand
    {
        #region Field
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _eventFactory;
        private readonly IGuidFactory _guidFactory;
        private readonly ILoggerFactory _loggerFactory;
        protected readonly ILogger _logger;

        #region AsyncStream
        private IStreamProvider? _streamProvider;
        private IAsyncStream<SummaryEvent>? _summaryStream;
        private IAsyncStream<ScalarEvent>? _scalarStream;
        #endregion

        #region AsyncObserver
        private IAsyncObserver<SummaryEvent>? _summaryObserver;
        private IAsyncObserver<ScalarEvent>? _scalarObserver;
        #endregion

        #region SubscriptionHandle
        private StreamSubscriptionHandle<SummaryEvent>? _summaryHandle;
        private StreamSubscriptionHandle<ScalarEvent>? _scalarHandle;
        #endregion

        private Guid _currentAggregateId;
        private int _process;
        private int _completedCount;
        #endregion

        protected ScheduleUnitGrain(IScheduleSessionFactory<TRequest, TCommand> sessionFactory, ISchduleSentEventFactory<TSentEvent, TRequest> eventFactory,
            IGuidFactory guidFactory, ILoggerFactory loggerFactory)
        {
            _sessionFactory = sessionFactory;
            _eventFactory = eventFactory;
            _guidFactory = guidFactory;
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _currentAggregateId = State.CurrentAggregateId;
            _process = State.Process;
            _completedCount = State.CompletedCount;
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            var scheduleId = this.GetPrimaryKey();

            #region AsyncStream
            _summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, scheduleId);
            _scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, scheduleId);
            #endregion

            #region AsyncObserver
            _summaryObserver = new SummaryEventObserver(this, _loggerFactory.CreateLogger<SummaryEventObserver>());
            _scalarObserver = new ScalarEventObserver(this, _loggerFactory.CreateLogger<ScalarEventObserver>());
            #endregion

            #region SubscriptionHandle
            _summaryHandle = await _summaryStream.SubscribeAsync(_summaryObserver);
            _scalarHandle = await _scalarStream.SubscribeAsync(_scalarObserver);
            #endregion
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            State.CurrentAggregateId = _currentAggregateId;
            State.Process = _process;
            State.CompletedCount = _completedCount;
            await WriteStateAsync();

            #region SubscriptionHandle
            if (_summaryHandle is not null)
                await _summaryHandle.UnsubscribeAsync();

            if (_scalarHandle is not null)
                await _scalarHandle.UnsubscribeAsync();
            #endregion

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }

        private Task OnAggregateCompletedAsync()
        {

            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task ExecuteAsync()
        {
            if (Interlocked.CompareExchange(ref _process, 1, 0) == 0)
            {
                Restart();
                var unitId = this.GetPrimaryKey();
                var aggregateId = await _guidFactory.NextAsync();
                _currentAggregateId = aggregateId;
                var aggregateGrain = GrainFactory.GetGrain<IAggregateGrain>(aggregateId);
                await aggregateGrain.StartMeasurementAsync(unitId, State.Command.Sample, State.Command.BatchSize);

                await InternalScheduleAsync(State.Command, aggregateId, ExecuteSessionAsync);
            }
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Guid aggregateId, Func<TCommand, Guid, Task> sessionTask);

        [Rougamo<LoggingException>]
        private async Task ExecuteSessionAsync(TCommand command, Guid aggregateId)
        {
            if (Interlocked.CompareExchange(ref _process, 1, 1) == 1)
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
                    await sentStream.OnCompletedAsync();
                }
            }
        }

        public Task ModifyUnitAsync(TCommand command)
        {
            if (Interlocked.CompareExchange(ref _process, 0, 0) == 0)
            {
                Restart();
                State.Command = command;
            }
            return Task.CompletedTask;
        }

        public Task ClearUnitAsync()
        {
            if (Interlocked.CompareExchange(ref _process, 0, 0) == 0)
            {
                _completedCount = 0;
                return ClearStateAsync();
            }
            else
                return Task.CompletedTask;
        }

        public Task CancelAsync()
        {
            if (Interlocked.CompareExchange(ref _process, 0, 1) == 1)
            {
                Restart();
                return Task.CompletedTask;
            }
            else
                return Task.CompletedTask;
        }

        protected void Restart()
        {

            _completedCount = 0;
            State.Summary = default;
            State.Scalars = [];
            State.CompletedCount = 0;
            State.Process = 0;
            State.CurrentAggregateId = default;
        }
    }
}
