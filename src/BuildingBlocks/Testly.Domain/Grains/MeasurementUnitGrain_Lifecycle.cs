using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;
using TorchSharp;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription]
    internal partial class MeasurementUnitGrain : Grain<MeasurementUnitState>, 
        IDomainEventAsyncObserver<MeasurementUnitEvent>,
        IDomainEventAsyncObserver<MeasurementUnitCancelEvent>,
        IRemindable
    {
        private readonly ILogger _logger;
        private readonly IGuidFactory _factory;
        private readonly IAsyncObserver<MeasurementUnitEvent> _unitObserver;
        private readonly IAsyncObserver<MeasurementUnitCancelEvent> _cancelObserver;
        private Guid? _unitId;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<MeasurementUnitEvent>? _measurementUnitStream;
        private IAsyncStream<MeasurementUnitCancelEvent>? _measurementUnitCancelStream;
        private StreamSubscriptionHandle<MeasurementUnitEvent>? _unitHandle;
        private StreamSubscriptionHandle<MeasurementUnitCancelEvent>? _cancelHandle;
        private torch.Tensor? _receivedMeasurement;

        private Guid UnitId
            => _unitId ??= this.GetPrimaryKey();

        private IStreamProvider StreamProvider
            => _streamProvider ??= this.GetStreamProvider(nameof(Stream));

        private IAsyncStream<MeasurementUnitEvent> MeasurementUnitStream
            => _measurementUnitStream ??= StreamProvider.GetStream<MeasurementUnitEvent>(UnitId);

        private IAsyncStream<MeasurementUnitCancelEvent> MeasurementUnitCancelStream
            => _measurementUnitCancelStream ??= StreamProvider.GetStream<MeasurementUnitCancelEvent>(UnitId);

        private torch.Tensor ReceivedMeasurement
            => State.ReceivedMeasurement is null ? 
            torch.empty(State.Sample, torch.ScalarType.Float32) :
            torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);

        public MeasurementUnitGrain(ILogger<MeasurementUnitGrain> logger, 
            IGuidFactory factory, 
            IAsyncObserver<MeasurementUnitEvent> unitObserver, 
            IAsyncObserver<MeasurementUnitCancelEvent> cancelObserver)
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

            _unitHandle = await MeasurementUnitStream.SubscribeAsync(_unitObserver);
            _cancelHandle = await MeasurementUnitCancelStream.SubscribeAsync(_cancelObserver);
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                State.ApplyRent(ReceivedMeasurement);
                ReceivedMeasurement.bytes.CopyTo(State.ReceivedMeasurement);
                await WriteStateAsync();
                State.ApplyReturn();
            }
            else if (State.CurrentState != ScheduledNodeCurrentState.None)
                await ClearStateAsync();

            DisposeTensorSetNull(ref _receivedMeasurement);

            await UnsubscribeSetNullAsync(ref _unitHandle);
            await UnsubscribeSetNullAsync(ref _cancelHandle);
            
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(MeasurementUnitGrain), this.GetPrimaryKey(), reason);
        }
    }
}
