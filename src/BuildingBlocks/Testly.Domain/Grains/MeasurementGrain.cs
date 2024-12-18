using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;
using TorchSharp;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains
{
    [GrainWithGuidKey]
    [StreamProvider]
    internal sealed partial class MeasurementGrain : Grain<MeasurementState>,
        IEventObserver<MeasurementExecuteEvent>,
        IEventObserver<MeasurementCompletedEvent>,
        IEventObserver<MeasurementCancelEvent>,
        IRemindable
    {
        private readonly ILogger _logger;
        
        private torch.Tensor? _receivedMeasurement;
        private torch.Tensor ReceivedMeasurement
            => _receivedMeasurement ??= State.ReceivedMeasurement is null ? 
            torch.empty(State.Sample, torch.ScalarType.Float32) :
            torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);

        public MeasurementGrain(ILogger<MeasurementGrain> logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (State.CurrentState == NodeCurrentState.Executing)
            {
                State.ApplyRent(ReceivedMeasurement);
                ReceivedMeasurement.bytes.CopyTo(State.ReceivedMeasurement);
                await WriteStateAsync();
                State.ApplyReturn();
            }
            else if (State.CurrentState != NodeCurrentState.None)
            {
                await ClearStateAsync();

                var reminder = await this.GetReminder(nameof(MeasurementGrain));
                if (reminder is not null)
                    await this.UnregisterReminder(reminder);
            }
            
            DisposeTensorSetNull(ref _receivedMeasurement);

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(MeasurementGrain), this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(MeasurementExecuteEvent item)
        {
            if (State.CurrentState != NodeCurrentState.Executing)
            {
                await this.RegisterOrUpdateReminder(nameof(MeasurementGrain), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
                State.ApplyExecute(item);
            }
        }

        public Task OnNextAsync(MeasurementCompletedEvent item)
        {
            if (State.CurrentState == NodeCurrentState.Executing)
            {
                var time = Convert.ToSingle((item.EndTime - item.StartTime).TotalMilliseconds);
                ReceivedMeasurement[State.ReceivedSample] = time;
                State.ApplyNext(item);
            }

            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(MeasurementCancelEvent item)
        {
            if (State.CurrentState == NodeCurrentState.Executing)
            {
                var reminder = await this.GetReminder(nameof(MeasurementGrain));
                if (reminder is not null)
                    await this.UnregisterReminder(reminder);
                State.ApplyCancelled();
            }
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == nameof(MeasurementGrain)
                && (DateTime.UtcNow - State.LastPublish) > TimeSpan.FromMinutes(2))
            {
                if (State.CurrentState == NodeCurrentState.Executing)
                    await OnCompletedAsync();
                else
                {
                    var reminder = await this.GetReminder(nameof(MeasurementGrain));
                    if (reminder is not null)
                        await this.UnregisterReminder(reminder);
                }
            }
        }

        [Rougamo<LoggingException>]
        private async Task OnCompletedAsync()
        {
            if (State.Root != default)
                await PublishAsync();

            var reminder = await this.GetReminder(nameof(MeasurementGrain));
            if (reminder is not null)
                await this.UnregisterReminder(reminder);

            State.ApplyCompleted();
        }

        private async Task PublishAsync()
        {
            using (_ = torch.NewDisposeScope())
            {
                var start = 0;
                var end = 0;

                var scalarResultEventStream = StreamProvider.GetStream<ScalarEvent>(nameof(MeasurementGrain), State.Root);

                while (end < State.ReceivedSample)
                {
                    end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                    await PublishScalarAsync(scalarResultEventStream, State.Root,
                        start / State.BatchSize, start, end);

                    start += State.BatchSize;
                }

                var summaryResultEventStream = StreamProvider.GetStream<SummaryEvent>(nameof(MeasurementGrain), State.Root);
                await PublishSummaryAsync(summaryResultEventStream, State.Root,
                    State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);

                var nodeCompletedEventStream = StreamProvider.GetStream<NodeCompletedEvent>(GrainId);
                await nodeCompletedEventStream.OnNextAsync(new NodeCompletedEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = GrainId
                });
            }
        }

        private Task PublishScalarAsync(IAsyncStream<ScalarEvent> scalarResultEventStream, Guid storageId,
            int index, int start, int end)
        {
            var tempSlicedTensor = ReceivedMeasurement[torch.TensorIndex.Slice(start, end)];
            var scalarEvent = new ScalarEvent
            {
                UnitName = State.UnitName,
                PublisherId = GrainId,
                SubscriberId = storageId,
                Index = index,
                Avg = torch.mean(tempSlicedTensor).item<float>(),
                Min = torch.min(tempSlicedTensor).item<float>(),
                Max = torch.max(tempSlicedTensor).item<float>(),
                Mid = torch.median(tempSlicedTensor).item<float>(),
                Std = torch.std(tempSlicedTensor).item<float>()
            };
            return scalarResultEventStream.OnNextAsync(scalarEvent);
        }

        private Task PublishSummaryAsync(IAsyncStream<SummaryEvent> summaryResultEventStream, Guid storageId,
            int receivedSample, int sample, DateTime startTime, DateTime endTime)
        {
            var slicedTensor = ReceivedMeasurement[torch.TensorIndex.Slice(0, receivedSample)];
            var summaryEvent = new SummaryEvent
            {
                UnitName = State.UnitName,
                PublisherId = GrainId,
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
            return summaryResultEventStream.OnNextAsync(summaryEvent);
        }
    }
}
