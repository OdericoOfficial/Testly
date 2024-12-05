using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States;
using TorchSharp;
using TorchSharp.Modules;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(nameof(AggregateEvent))]
    internal class AggregateGrain : Grain<AggregateState>, IAggregateGrain, IAsyncObserver<AggregateEvent>, IRemindable
    {
        private readonly ILogger<AggregateGrain> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<AggregateEvent>? _aggregateStream;
        private StreamSubscriptionHandle<AggregateEvent>? _subscriptionHandle;
        private torch.Tensor? _sentMeasurement;
        private torch.Tensor? _receivedMeasurement;

        public AggregateGrain(ILogger<AggregateGrain> logger)
        {
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _aggregateStream = _streamProvider.GetStream<AggregateEvent>(nameof(AggregateEvent), this.GetPrimaryKey());
            _subscriptionHandle = await _aggregateStream.SubscribeAsync(this);

            if (State.SentMeasurement.Length > 0)
                _sentMeasurement = torch.frombuffer(State.SentMeasurement, torch.ScalarType.Float32);

            if (State.ReceivedMeasurement.Length > 0)
                _receivedMeasurement = torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);
        }

        //TODO: make write more safety
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            try
            {
                if (_sentMeasurement is not null)
                {
                    State.SentMeasurement = _sentMeasurement.bytes.ToArray();
                    _sentMeasurement.Dispose();
                }

                if (_receivedMeasurement is not null)
                {
                    State.ReceivedMeasurement = _receivedMeasurement.bytes.ToArray();
                    _receivedMeasurement.Dispose();
                }

                await WriteStateAsync();

                _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                    nameof(AggregateGrain), this.GetPrimaryKey(), reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in {MethodName} from {GrainName} {GrainId}",
                    nameof(OnDeactivateAsync), nameof(AggregateGrain), this.GetPrimaryKey());
            }
        }

        public async Task StartMeasurementAsync(int sample, int batchSize)
        {
            State.Sample = sample;
            State.BatchSize = batchSize;

            if (_sentMeasurement is null)
                _sentMeasurement = torch.empty(sample, torch.ScalarType.Float32);
            if (_receivedMeasurement is null)
                _receivedMeasurement = torch.empty(sample, torch.ScalarType.Float32);
            
            State.SentMeasurement = _sentMeasurement.bytes.ToArray();
            State.ReceivedMeasurement = _receivedMeasurement.bytes.ToArray();

            await this.RegisterOrUpdateReminder(nameof(AggregateState), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
        }

        //TODO: may publish progress
        public Task OnNextAsync(AggregateEvent item, StreamSequenceToken? token = null)
        {
            var time = Convert.ToSingle((item.ReceivedTime - item.SendingTime).TotalMilliseconds);
            State.ReceivedSample++;

            if (_sentMeasurement is not null)
                _sentMeasurement[item.SentIndex] = time;

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

        //TODO: save summary write
        public Task OnCompletedAsync()
            => Task.CompletedTask;

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == nameof(AggregateState) && _aggregateStream is not null)
                await _aggregateStream.OnCompletedAsync();
        }

        private void WriteToTensorboard(string logDir)
        {
            using (_ = torch.NewDisposeScope())
            {
                var writer = torch.utils.tensorboard.SummaryWriter();

                var start = 0;
                var end = 0;
                while (end < State.ReceivedSample)
                {
                    end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                    if (_sentMeasurement is not null)
                        AddScalar(writer, _sentMeasurement, nameof(AggregateState.SentMeasurement), start / State.BatchSize, start, end);

                    if (_receivedMeasurement is not null)
                        AddScalar(writer, _receivedMeasurement, nameof(AggregateState.ReceivedMeasurement), start / State.BatchSize, start, end);

                    start += State.BatchSize;
                }

                if (_sentMeasurement is not null)
                    AddSummary(writer, _sentMeasurement, nameof(AggregateState.SentMeasurement),
                        State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);

                if (_receivedMeasurement is not null)
                    AddSummary(writer, _receivedMeasurement, nameof(AggregateState.SentMeasurement),
                            State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);
            }
        }

        private void AddScalar(SummaryWriter boardWriter, torch.Tensor tensor, string prefix, int index, int start, int end)
        {
            var tempSlicedTensor = tensor[torch.TensorIndex.Slice(start, end)];
            boardWriter.add_scalar($"{prefix}-Avg",
                torch.mean(tempSlicedTensor).item<float>(), index);
            boardWriter.add_scalar($"{prefix}-Min",
                torch.min(tempSlicedTensor).item<float>(), index);
            boardWriter.add_scalar($"{prefix}-Max",
                torch.max(tempSlicedTensor).item<float>(), index);
            boardWriter.add_scalar($"{prefix}-Mid",
                torch.median(tempSlicedTensor).item<float>(), index);
            boardWriter.add_scalar($"{prefix}-Std",
                torch.std(tempSlicedTensor).item<float>(), index);
        }

        private void AddSummary(SummaryWriter boardWriter, torch.Tensor tensor, string tag,
            int receivedSample, int sample, DateTime startTime, DateTime endTime)
        {
            var slicedTensor = tensor[torch.TensorIndex.Slice(0, receivedSample)];
            var avg = torch.mean(slicedTensor).item<float>();
            var mid = torch.median(slicedTensor).item<float>();
            var min = torch.min(slicedTensor).item<float>();
            var max = torch.max(slicedTensor).item<float>();
            var std = torch.std(slicedTensor).item<float>();
            var error = ((float)(sample - receivedSample) * 100) / sample;
            var tps = 1000 * sample / (endTime - startTime).TotalMilliseconds;
            var quantile90 = torch.quantile(slicedTensor, 0.9f).item<float>();
            var quantile95 = torch.quantile(slicedTensor, 0.95f).item<float>();
            var quantile99 = torch.quantile(slicedTensor, 0.99f).item<float>();

            boardWriter.add_text(tag, $"Sample: {sample}", 0);
            boardWriter.add_text(tag, $"Avg: {avg}", 1);
            boardWriter.add_text(tag, $"Mid: {mid}", 2);
            boardWriter.add_text(tag, $"Min: {min}", 3);
            boardWriter.add_text(tag, $"Max: {max}", 4);
            boardWriter.add_text(tag, $"Std: {std}", 5);
            boardWriter.add_text(tag, $"Error: {error}", 6);
            boardWriter.add_text(tag, $"TPS: {tps}", 7);
            boardWriter.add_text(tag, $"90 Quantile: {quantile90}", 8);
            boardWriter.add_text(tag, $"95 Quantile: {quantile95}", 9);
            boardWriter.add_text(tag, $"99 Quantile: {quantile99}", 10);
        }
    }
}
