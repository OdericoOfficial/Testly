using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduleUnitGrain<TSentEvent, TRequest, TCommand> : Grain<ScheduleUnitState<TCommand>>, IScheduleUnitGrain<TCommand>,
        IDomainEventAsyncObserver<StorageCompletedEvent>, IDomainEventAsyncObserver<ScheduleUnitCancelEvent>, IDomainEventAsyncObserver<ScheduleUnitExecuteEvent>
        where TSentEvent : struct, ISentEvent
        where TCommand : struct, IModifyUnitCommand
    {
        protected readonly ILogger _logger;
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _eventFactory;
        private readonly IAsyncObserver<ScheduleUnitExecuteEvent> _unitExecuteObserver;
        private readonly IAsyncObserver<ScheduleUnitCancelEvent> _unitCancelObserver;
        private readonly IAsyncObserver<StorageCompletedEvent> _storageCompletedObserver;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<ScheduleUnitExecuteEvent>? _unitExecuteStream;
        private IAsyncStream<ScheduleUnitCancelEvent>? _unitCancelStream;
        private IAsyncStream<StorageCompletedEvent>? _storageCompletedStream;
        private IAsyncStream<AggregateUnitCancelEvent>? _aggregateUnitCancelStream;
        private IAsyncStream<ScheduleUnitCompletedEvent>? _scheduleUnitCompletedStream;
        private StreamSubscriptionHandle<ScheduleUnitExecuteEvent>? _unitExecuteHandle;
        private StreamSubscriptionHandle<ScheduleUnitCancelEvent>? _unitCancelHandle;
        private StreamSubscriptionHandle<StorageCompletedEvent>? _storageCompletedHandle;
        
        protected ScheduleUnitGrain(IScheduleSessionFactory<TRequest, TCommand> sessionFactory, ISchduleSentEventFactory<TSentEvent, TRequest> eventFactory,
            IAsyncObserver<StorageCompletedEvent> storageCompletedObserver, IAsyncObserver<ScheduleUnitCancelEvent> unitCancelObserver,
            IAsyncObserver<ScheduleUnitExecuteEvent> unitExecuteObserver, ILogger logger)
        {
            _sessionFactory = sessionFactory;
            _eventFactory = eventFactory;
            _storageCompletedObserver = storageCompletedObserver;
            _unitCancelObserver = unitCancelObserver;
            _unitExecuteObserver = unitExecuteObserver;
            _logger = logger;
        }
         
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var unitId = this.GetPrimaryKey();
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);

            if (State.Process == ScheduleUnitProcess.Running)
                await InitScopeAsync(unitId);
            else
            {
                _unitExecuteStream = _streamProvider.GetStream<ScheduleUnitExecuteEvent>(unitId);
                _unitExecuteHandle = await _unitExecuteStream.SubscribeAsync(_unitExecuteObserver);
            }
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.Process == ScheduleUnitProcess.Running)
                await ReleaseScopeAsync();
            else
                await _unitExecuteHandle!.UnsubscribeAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }

        public async Task ModifyAsync(TCommand command)
        {
            if (State.Process != ScheduleUnitProcess.Running)
            {
                State.ApplyModify(command);
                await WriteStateAsync();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task ClearAsync()
        {
            if (State.Process != ScheduleUnitProcess.Running)
            {
                await _unitExecuteHandle!.UnsubscribeAsync();
                await ClearStateAsync();
            }
        }

        public async Task OnNextAsync(ScheduleUnitExecuteEvent item)
        {
            if (State.Process != ScheduleUnitProcess.Running)
            {
                State.ApplyExecute();

                var unitId = this.GetPrimaryKey();
                await InitScopeAsync(unitId);
                await _unitExecuteHandle!.UnsubscribeAsync();

                var aggregateGrain = GrainFactory.GetGrain<IAggregateUnitGrain>(unitId);
                await aggregateGrain.ExecuteAsync(State.Command.Sample, State.Command.BatchSize);

                await InternalScheduleAsync(State.Command, unitId, ExecuteSessionAsync);
            }
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Guid unitId, Func<TCommand, Guid, Task> sessionTask);

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

        public async Task OnNextAsync(StorageCompletedEvent item)
        {
            if (State.Process == ScheduleUnitProcess.Running)
            {
                var unitId = this.GetPrimaryKey();
                State.ApplyComplete(item.CompletedTime);

                await _scheduleUnitCompletedStream!.OnNextAsync(new ScheduleUnitCompletedEvent
                {
                    PublisherId = unitId,
                    SubscriberId = State.Command.LayerId
                });

                _unitExecuteStream = _streamProvider.GetStream<ScheduleUnitExecuteEvent>(unitId);
                _unitExecuteHandle = await _unitExecuteStream.SubscribeAsync(_unitExecuteObserver);

                await ReleaseScopeAsync();
                await WriteStateAsync();
            }
        }

        public async Task OnNextAsync(ScheduleUnitCancelEvent item)
        {
            if (State.Process == ScheduleUnitProcess.Running)
            {
                var unitId = this.GetPrimaryKey();
                State.ApplyCancel();

                await _aggregateUnitCancelStream!.OnNextAsync(new AggregateUnitCancelEvent
                {
                    PublisherId = unitId,
                    SubscriberId = unitId
                });

                _unitExecuteStream = _streamProvider.GetStream<ScheduleUnitExecuteEvent>(unitId);
                _unitExecuteHandle = await _unitExecuteStream.SubscribeAsync(_unitExecuteObserver);

                await ReleaseScopeAsync();
                await WriteStateAsync();
            }
        }

        private async Task InitScopeAsync(Guid unitId)
        {
            _storageCompletedStream = _streamProvider!.GetStream<StorageCompletedEvent>(Constants.DefaultStorageCompletedNamespace, unitId);
            _unitCancelStream = _streamProvider.GetStream<ScheduleUnitCancelEvent>(Constants.DefaultScheduleUnitCancelNamesapce, unitId);
            _aggregateUnitCancelStream = _streamProvider.GetStream<AggregateUnitCancelEvent>(Constants.DefaultAggregateUnitCancelNamespace, unitId);
            _scheduleUnitCompletedStream = _streamProvider.GetStream<ScheduleUnitCompletedEvent>(Constants.DefaultSchduleUnitCompletedNamespace, unitId);
            _storageCompletedHandle = await _storageCompletedStream.SubscribeAsync(_storageCompletedObserver);
            _unitCancelHandle = await _unitCancelStream.SubscribeAsync(_unitCancelObserver);
        }

        private async Task ReleaseScopeAsync()
        {
            await _storageCompletedHandle!.UnsubscribeAsync();
            await _unitCancelHandle!.UnsubscribeAsync();
        }
    }
}
