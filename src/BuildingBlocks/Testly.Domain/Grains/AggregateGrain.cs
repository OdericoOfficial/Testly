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
    internal class AggregateGrain : Grain<AggregateState>, IAggregateGrain, IAsyncObserver<AggregateEvent>, IRemindable
    {
        private readonly ILogger<AggregateGrain> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<AggregateEvent>? _aggregateStream;
        private IAsyncStream<ScalarEvent>? _scalarStream;
        private IAsyncStream<SummaryEvent>? _summaryStream;
        private torch.Tensor? _receivedMeasurement;

        public AggregateGrain(ILogger<AggregateGrain> logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _aggregateStream = _streamProvider.GetStream<AggregateEvent>(Constants.DefaultAggregateNamespace, this.GetPrimaryKey());
            _scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, this.GetPrimaryKey());
            _summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, this.GetPrimaryKey()); 
            await _aggregateStream.SubscribeAsync(this);

            if (State.ReceivedMeasurement.Length > 0)
                _receivedMeasurement = torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (_receivedMeasurement is not null)
            {
                State.ReceivedMeasurement = _receivedMeasurement.bytes.ToArray();
                _receivedMeasurement.Dispose();
            }

            await WriteStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateGrain), this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public async Task StartMeasurementAsync(int sample, int batchSize)
        {
            State.Sample = sample;
            State.BatchSize = batchSize;

            if (_receivedMeasurement is null)
                _receivedMeasurement = torch.empty(sample, torch.ScalarType.Float32);

            State.ReceivedMeasurement = _receivedMeasurement.bytes.ToArray();

            await this.RegisterOrUpdateReminder(nameof(AggregateState), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
        }

        public Task OnNextAsync(AggregateEvent item, StreamSequenceToken? token = null)
        {
            var time = Convert.ToSingle((item.ReceivedTime - item.SendingTime).TotalMilliseconds);
            State.ReceivedSample++;

            if (_receivedMeasurement is not null)
                _receivedMeasurement[item.ReceivedIndex] = time;

            if (State.StartTime > item.SendingTime)
                State.StartTime = item.SendingTime;

            if (State.EndTime < item.ReceivedTime)
                State.EndTime = item.ReceivedTime;

            State.LastPublish = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                nameof(AggregateEvent), nameof(AggregateGrain), this.GetPrimaryKey());
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task OnCompletedAsync()
        {
            await PublishAsync();
            var reminder = await this.GetReminder(Constants.DefaultAggregateCompleteReminder);
            if (reminder is not null)
                await this.UnregisterReminder(reminder);
            await ClearStateAsync();
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == Constants.DefaultAggregateCompleteReminder 
                && _aggregateStream is not null 
                && (DateTime.UtcNow - State.LastPublish) > TimeSpan.FromMinutes(2))
                await _aggregateStream.OnCompletedAsync();
        }

        private async Task PublishAsync()
        {
            using (_ = torch.NewDisposeScope())
            {
                var start = 0;
                var end = 0;
                while (end < State.ReceivedSample)
                {
                    end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                    if (_receivedMeasurement is not null && _scalarStream is not null)
                        await PublishScalarAsync(_receivedMeasurement, start / State.BatchSize, start, end);

                    start += State.BatchSize;
                }

                if (_receivedMeasurement is not null && _summaryStream is not null)
                    await PublishSummaryAsync(_receivedMeasurement, State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);
            }
        }

        [Rougamo<LoggingException>]
        private Task PublishScalarAsync(torch.Tensor tensor, int index, int start, int end)
        {
            var tempSlicedTensor = tensor[torch.TensorIndex.Slice(start, end)];
            var scalarEvent = new ScalarEvent
            {
                Index = index,
                Avg = torch.mean(tempSlicedTensor).item<float>(),
                Min = torch.min(tempSlicedTensor).item<float>(),
                Max = torch.max(tempSlicedTensor).item<float>(),
                Mid = torch.median(tempSlicedTensor).item<float>(),
                Std = torch.std(tempSlicedTensor).item<float>()
            };
            return _scalarStream!.OnNextAsync(scalarEvent);
        }

        [Rougamo<LoggingException>]
        private Task PublishSummaryAsync(torch.Tensor tensor, int receivedSample, int sample, 
            DateTime startTime, DateTime endTime)
        {
            var slicedTensor = tensor[torch.TensorIndex.Slice(0, receivedSample)];
            var summaryEvent = new SummaryEvent
            {
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
            return _summaryStream!.OnNextAsync(summaryEvent);
        }
    }
}
