using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using TorchSharp;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(Constants.DefaultAggregateUnitNamespace)]
    [ImplicitStreamSubscription(Constants.DefaultAggregateUnitCancelNamespace)]
    internal sealed class AggregateUnitGrain : Grain<AggregateUnitState>, IAggregateUnitGrain, IDomainEventAsyncObserver<AggregateUnitEvent>,
        IDomainEventAsyncObserver<AggregateUnitCancelEvent>, IRemindable
    {
        private readonly ILogger _logger;
        private readonly IGuidFactory _factory;
        private readonly IAsyncObserver<AggregateUnitEvent> _unitObserver;
        private readonly IAsyncObserver<AggregateUnitCancelEvent> _cancelObserver;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<AggregateUnitEvent>? _aggregateStream;
        private IAsyncStream<AggregateUnitCancelEvent>? _cancelStream;
        private StreamSubscriptionHandle<AggregateUnitEvent>? _unitHandle;
        private StreamSubscriptionHandle<AggregateUnitCancelEvent>? _cancelHandle;
        private torch.Tensor? _receivedMeasurement;
        
        public AggregateUnitGrain(ILogger<AggregateUnitGrain> logger, IGuidFactory factory, IAsyncObserver<AggregateUnitEvent> unitObserver, IAsyncObserver<AggregateUnitCancelEvent> cancelObserver)
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
            if (State.Process == AggregateUnitProcess.Running)
            {
                var unitId = this.GetPrimaryKey();
                _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
                _receivedMeasurement = torch.frombuffer(State.ReceivedMeasurement!, torch.ScalarType.Float32);
                _aggregateStream = _streamProvider.GetStream<AggregateUnitEvent>(Constants.DefaultAggregateUnitNamespace, unitId);
                _cancelStream = _streamProvider.GetStream<AggregateUnitCancelEvent>(Constants.DefaultAggregateUnitCancelNamespace, unitId);
                _unitHandle = await _aggregateStream.SubscribeAsync(_unitObserver);
                _cancelHandle = await _cancelStream.SubscribeAsync(_cancelObserver);
            }
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.Process == AggregateUnitProcess.Running)
            {
                State.ApplyRent(_receivedMeasurement!);
                _receivedMeasurement!.bytes.CopyTo(State.ReceivedMeasurement);
                await WriteStateAsync();
                State.ApplyReturn();

                _receivedMeasurement.Dispose();
                _receivedMeasurement = null;

                await _unitHandle!.UnsubscribeAsync();
                await _cancelHandle!.UnsubscribeAsync();
            }
            else if (State.Process != AggregateUnitProcess.None)
                await ClearStateAsync();
            
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateUnitGrain), this.GetPrimaryKey(), reason);
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public async Task ExecuteAsync(int sample, int batchSize)
        {
            if (State.Process != AggregateUnitProcess.Running)
            {
                var unitId = this.GetPrimaryKey();
                _receivedMeasurement = torch.empty(sample, torch.ScalarType.Float32);
                State.ApplyExecute(sample, batchSize);

                _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
                _aggregateStream = _streamProvider.GetStream<AggregateUnitEvent>(Constants.DefaultAggregateUnitNamespace, unitId);
                _cancelStream = _streamProvider.GetStream<AggregateUnitCancelEvent>(Constants.DefaultAggregateUnitCancelNamespace, unitId);
                _unitHandle = await _aggregateStream.SubscribeAsync(_unitObserver);
                _cancelHandle = await _cancelStream.SubscribeAsync(_cancelObserver);

                await this.RegisterOrUpdateReminder(Constants.DefaultAggregateCompletedReminder, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
            }
        }

        public Task OnNextAsync(AggregateUnitEvent item)
        {
            if (State.Process == AggregateUnitProcess.Running)
            {
                var time = Convert.ToSingle((item.EndTime - item.StartTime).TotalMilliseconds);
                _receivedMeasurement![State.ReceivedSample] = time;
                State.ApplyNext(item);
            }

            return Task.CompletedTask;
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public async Task OnNextAsync(AggregateUnitCancelEvent item)
        {
            if (State.Process == AggregateUnitProcess.Running)
            {
                State.ApplyCancel();

                var reminder = await this.GetReminder(Constants.DefaultAggregateCompletedReminder);
                if (reminder is not null)
                    await this.UnregisterReminder(reminder);

                _receivedMeasurement!.Dispose();
                _receivedMeasurement = null;
                
                await _unitHandle!.UnsubscribeAsync();
                await _cancelHandle!.UnsubscribeAsync();
                
                await WriteStateAsync();
            }
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == Constants.DefaultAggregateCompletedReminder
                && (DateTime.UtcNow - State.LastPublish) > TimeSpan.FromMinutes(2)
                && State.Process == AggregateUnitProcess.Running)
                await OnCompletedAsync();
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        private async Task OnCompletedAsync()
        {
            await PublishAsync();

            State.ApplyCompleted();

            var reminder = await this.GetReminder(Constants.DefaultAggregateCompletedReminder);
            if (reminder is not null)
                await this.UnregisterReminder(reminder);

            _receivedMeasurement!.Dispose();
            _receivedMeasurement = null;

            await _unitHandle!.UnsubscribeAsync();
            await _cancelHandle!.UnsubscribeAsync();

            await WriteStateAsync();
        }

        private async Task PublishAsync()
        {
            using (_ = torch.NewDisposeScope())
            {
                var start = 0;
                var end = 0;
                var unitId = this.GetPrimaryKey();
                var storageId = await _factory.NextAsync();
                var scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, storageId);

                while (end < State.ReceivedSample)
                {
                    end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                    if (_receivedMeasurement is not null)
                        await PublishScalarAsync(scalarStream!, _receivedMeasurement,
                            unitId, storageId, start / State.BatchSize, start, end);

                    start += State.BatchSize;
                }

                var summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, storageId);
                await PublishSummaryAsync(summaryStream!, _receivedMeasurement!,
                    unitId, storageId, State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);
            }
        }

        private Task PublishScalarAsync(IAsyncStream<ScalarEvent> scalarStream, torch.Tensor tensor,
            Guid unitId, Guid storageId, int index, int start, int end)
        {
            var tempSlicedTensor = tensor[torch.TensorIndex.Slice(start, end)];
            var scalarEvent = new ScalarEvent
            {
                PublisherId = unitId,
                SubscriberId = storageId,
                Index = index,
                Avg = torch.mean(tempSlicedTensor).item<float>(),
                Min = torch.min(tempSlicedTensor).item<float>(),
                Max = torch.max(tempSlicedTensor).item<float>(),
                Mid = torch.median(tempSlicedTensor).item<float>(),
                Std = torch.std(tempSlicedTensor).item<float>()
            };
            return scalarStream.OnNextAsync(scalarEvent);
        }

        private Task PublishSummaryAsync(IAsyncStream<SummaryEvent> summaryStream, torch.Tensor tensor,
            Guid unitId, Guid storageId, int receivedSample, int sample, DateTime startTime, DateTime endTime)
        {
            var slicedTensor = tensor[torch.TensorIndex.Slice(0, receivedSample)];
            var summaryEvent = new SummaryEvent
            {
                PublisherId = unitId,
                SubscriberId = storageId,
                StartTime = startTime,
                EndTime = endTime,
                Sample = sample,
                ReceivedSample = receivedSample,
                Avg = torch.mean(slicedTensor).item<float>(),
                Mid = torch.median(slicedTensor).item<float>(),
                Min = torch.min(slicedTensor).item<float>(),
                Max = torch.max(slicedTensor).item<float>(),
                Std = torch.std(slicedTensor).item<float>(),
                Error = ((float)(sample - receivedSample) * 100) / sample,
                TPS = (float)(sample / (endTime - startTime).TotalSeconds),
                Quantile90 = torch.quantile(slicedTensor, 0.9f).item<float>(),
                Quantile95 = torch.quantile(slicedTensor, 0.95f).item<float>(),
                Quantile99 = torch.quantile(slicedTensor, 0.99f).item<float>()
            };
            return summaryStream.OnNextAsync(summaryEvent);
        }
    } 
}
