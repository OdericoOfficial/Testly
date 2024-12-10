using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.States;
using TorchSharp;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(Constants.DefaultAggregateNamespace)]
    internal partial class AggregateGrain : Grain<AggregateState>, IAggregateGrain, IRemindable
    {
        #region Field
        private readonly ILogger<AggregateGrain> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private IStreamProvider? _streamProvider;

        #region AsyncStream
        private IAsyncStream<AggregateEvent>? _aggregateStream;
        private IAsyncStream<ScalarEvent>? _scalarStream;
        private IAsyncStream<SummaryEvent>? _summaryStream;
        #endregion

        #region AsyncObserver
        private IAsyncObserver<AggregateEvent>? _aggregateObserver;
        #endregion

        #region SubscriptionHandle
        private StreamSubscriptionHandle<AggregateEvent>? _aggregateHandle;
        #endregion

        private torch.Tensor? _receivedMeasurement;
        #endregion

        public AggregateGrain(ILogger<AggregateGrain> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (State.ReceivedMeasurement.Length > 0)
                _receivedMeasurement = torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);

            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            var aggregateId = this.GetPrimaryKey();

            #region AsyncStream
            _aggregateStream = _streamProvider.GetStream<AggregateEvent>(Constants.DefaultAggregateNamespace, aggregateId);
            if (State.UnitId != default)
            {
                _scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, State.UnitId);
                _summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, State.UnitId);
            }
            #endregion

            #region AsyncObserver
            _aggregateObserver = new AggregateEventObserver(this, _loggerFactory.CreateLogger<AggregateEventObserver>());
            #endregion

            #region SubscriptionHandle
            _aggregateHandle = await _aggregateStream.SubscribeAsync(_aggregateObserver);
            #endregion
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _receivedMeasurement?.Dispose();
            await ClearStateAsync();

            #region SubscriptionHandle
            if (_aggregateHandle is not null)
                await _aggregateHandle.UnsubscribeAsync();
            #endregion

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateGrain), this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public async Task StartMeasurementAsync(Guid unitId, int sample, int batchSize)
        {
            State.Sample = sample;
            State.BatchSize = batchSize;

            if (_receivedMeasurement is null)
                _receivedMeasurement = torch.empty(sample, torch.ScalarType.Float32);

            State.ReceivedMeasurement = _receivedMeasurement.bytes.ToArray();
            State.UnitId = unitId;

            if (_streamProvider is not null)
            {
                _scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, unitId);
                _summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, unitId);
            }

            await this.RegisterOrUpdateReminder(nameof(AggregateState), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == Constants.DefaultAggregateCompleteReminder
                && (DateTime.UtcNow - State.LastPublish) > TimeSpan.FromMinutes(2)
                && _aggregateStream is not null)
                return _aggregateStream.OnCompletedAsync();
            else
                return Task.CompletedTask;
        }

        private async Task PublishAsync()
        {
            using (_ = torch.NewDisposeScope())
            {
                var start = 0;
                var end = 0;

                if (_scalarStream is not null)
                {
                    while (end < State.ReceivedSample)
                    {
                        end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                        if (_receivedMeasurement is not null)
                            await PublishScalarAsync(_scalarStream, _receivedMeasurement,
                                start / State.BatchSize, start, end);

                        start += State.BatchSize;
                    }

                    await _scalarStream.OnCompletedAsync();
                }


                if (_receivedMeasurement is not null
                    && _summaryStream is not null)
                {
                    await PublishSummaryAsync(_summaryStream, _receivedMeasurement,
                        State.ReceivedSample, State.Sample,
                        State.StartTime, State.EndTime);
                    await _summaryStream.OnCompletedAsync();
                }
            }
        }

        [Rougamo<LoggingException>]
        private Task PublishScalarAsync(IAsyncStream<ScalarEvent> scalarStream, torch.Tensor tensor,
            int index, int start, int end)
        {
            var tempSlicedTensor = tensor[torch.TensorIndex.Slice(start, end)];
            var scalarEvent = new ScalarEvent
            {
                AggregateId = this.GetPrimaryKey(),
                Index = index,
                Avg = torch.mean(tempSlicedTensor).item<float>(),
                Min = torch.min(tempSlicedTensor).item<float>(),
                Max = torch.max(tempSlicedTensor).item<float>(),
                Mid = torch.median(tempSlicedTensor).item<float>(),
                Std = torch.std(tempSlicedTensor).item<float>()
            };
            return scalarStream.OnNextAsync(scalarEvent);
        }

        [Rougamo<LoggingException>]
        private Task PublishSummaryAsync(IAsyncStream<SummaryEvent> summaryStream, torch.Tensor tensor,
            int receivedSample, int sample,
            DateTime startTime, DateTime endTime)
        {
            var slicedTensor = tensor[torch.TensorIndex.Slice(0, receivedSample)];
            var summaryEvent = new SummaryEvent
            {
                StartTime = startTime,
                EndTime = endTime,
                AggregateId = this.GetPrimaryKey(),
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
