using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;
using TorchSharp;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(Constants.DefaultAggregateUnitNamespace)]
    [ImplicitStreamSubscription(Constants.DefaultAggregateUnitCancelNamespace)]
    internal partial class AggregateUnitGrain : Grain<AggregateUnitState>, 
        IAggregateUnitGrain, 
        IDomainEventAsyncObserver<AggregateUnitEvent>,
        IDomainEventAsyncObserver<AggregateUnitCancelEvent>,
        IRemindable
    {
        private readonly ILogger _logger;
        private readonly IGuidFactory _factory;
        private readonly IAsyncObserver<AggregateUnitEvent> _unitObserver;
        private readonly IAsyncObserver<AggregateUnitCancelEvent> _cancelObserver;
        private Guid? _unitId;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<AggregateUnitEvent>? _aggregateUnitStream;
        private IAsyncStream<AggregateUnitCancelEvent>? _aggregateUnitCancelStream;
        private StreamSubscriptionHandle<AggregateUnitEvent>? _unitHandle;
        private StreamSubscriptionHandle<AggregateUnitCancelEvent>? _cancelHandle;
        private torch.Tensor? _receivedMeasurement;

        private Guid UnitId
            => _unitId ??= this.GetPrimaryKey();

        private IStreamProvider StreamProvider
            => _streamProvider ??= this.GetStreamProvider(Constants.DefaultStreamProvider);

        private IAsyncStream<AggregateUnitEvent> AggregateUnitStream
            => _aggregateUnitStream ??= StreamProvider.GetStream<AggregateUnitEvent>(UnitId);

        private IAsyncStream<AggregateUnitCancelEvent> AggregateUnitCancelStream
            => _aggregateUnitCancelStream ??= StreamProvider.GetStream<AggregateUnitCancelEvent>(UnitId);

        private torch.Tensor ReceivedMeasurement
            => State.ReceivedMeasurement is null ? 
            torch.empty(State.Sample, torch.ScalarType.Float32) :
            torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);

        public AggregateUnitGrain(ILogger<AggregateUnitGrain> logger, 
            IGuidFactory factory, 
            IAsyncObserver<AggregateUnitEvent> unitObserver, 
            IAsyncObserver<AggregateUnitCancelEvent> cancelObserver)
        {
            _logger = logger;
            _factory = factory;
            _unitObserver = unitObserver;
            _cancelObserver = cancelObserver;
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

            _unitHandle = await AggregateUnitStream.SubscribeAsync(_unitObserver);
            _cancelHandle = await AggregateUnitCancelStream.SubscribeAsync(_cancelObserver);
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.CurrentState == ScheduledNodeState.Executing)
            {
                State.ApplyRent(ReceivedMeasurement);
                ReceivedMeasurement.bytes.CopyTo(State.ReceivedMeasurement);
                await WriteStateAsync();
                State.ApplyReturn();
            }
            else if (State.CurrentState != ScheduledNodeState.None)
                await ClearStateAsync();

            DisposeTensorSetNull(ref _receivedMeasurement);

            await UnsubscribeSetNullAsync(ref _unitHandle);
            await UnsubscribeSetNullAsync(ref _cancelHandle);
            
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateUnitGrain), this.GetPrimaryKey(), reason);
        }
    }
}
