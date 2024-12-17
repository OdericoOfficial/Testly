using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;
using TorchSharp;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains
{
    [GrainWithGuidKey]
    [StreamProvider]
    [ImplicitStreamSubscription]
    [ImplicitSubscribeAsyncStream<MeasurementUnitExecuteEvent>]
    [ImplicitSubscribeAsyncStream<MeasurementUnitCompletedEvent>]
    [ImplicitSubscribeAsyncStream<MeasurementUnitCancelEvent>]
    internal sealed partial class MeasurementUnitGrain : Grain<MeasurementUnitState>, 
        IDomainEventAsyncObserver<MeasurementUnitCompletedEvent>,
        IDomainEventAsyncObserver<MeasurementUnitExecuteEvent>,
        IDomainEventAsyncObserver<MeasurementUnitCancelEvent>,
        IRemindable
    {
        private readonly ILogger _logger;
        private readonly IGuidFactory _factory;

        private torch.Tensor? _receivedMeasurement;
        private torch.Tensor ReceivedMeasurement
            => _receivedMeasurement ??= State.ReceivedMeasurement is null ? 
            torch.empty(State.Sample, torch.ScalarType.Float32) :
            torch.frombuffer(State.ReceivedMeasurement, torch.ScalarType.Float32);

        public MeasurementUnitGrain(ILogger<MeasurementUnitGrain> logger, 
            IGuidFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await SubscribeAllAsync();
        }

        [Rougamo<LoggingException>]
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

            await UnsubscribeAllAsync();
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(MeasurementUnitGrain), this.GetPrimaryKey(), reason);
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(MeasurementUnitExecuteEvent item)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
            {
                await this.RegisterOrUpdateReminder(nameof(MeasurementUnitGrain), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
                State.ApplyExecute(item);
            }
        }

        public Task OnNextAsync(MeasurementUnitCompletedEvent item)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                var time = Convert.ToSingle((item.EndTime - item.StartTime).TotalMilliseconds);
                ReceivedMeasurement[State.ReceivedSample] = time;
                State.ApplyNext(item);
            }

            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task OnNextAsync(MeasurementUnitCancelEvent item)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing)
            {
                var reminder = await this.GetReminder(nameof(MeasurementUnitGrain));
                if (reminder is not null)
                    await this.UnregisterReminder(reminder);
                State.ApplyCancelled();
            }
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == nameof(MeasurementUnitGrain)
                && (DateTime.UtcNow - State.LastPublish) > TimeSpan.FromMinutes(2)
                && State.CurrentState == ScheduledNodeCurrentState.Executing)
                await OnCompletedAsync();
        }

        [Rougamo<LoggingException>]
        private async Task OnCompletedAsync()
        {
            await PublishAsync();

            var reminder = await this.GetReminder(nameof(MeasurementUnitGrain));
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
                var storageId = await _factory.NextAsync();
                var scalarResultEventStream = StreamProvider.GetStream<ScalarResultEvent>(nameof(MeasurementUnitGrain), storageId);

                while (end < State.ReceivedSample)
                {
                    end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                    await PublishScalarAsync(scalarResultEventStream, storageId,
                        start / State.BatchSize, start, end);

                    start += State.BatchSize;
                }

                var summaryResultEventStream = StreamProvider.GetStream<SummaryResultEvent>(nameof(MeasurementUnitGrain), storageId);
                await PublishSummaryAsync(summaryResultEventStream, storageId,
                    State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);
            }
        }

        private Task PublishScalarAsync(IAsyncStream<ScalarResultEvent> scalarResultEventStream, Guid storageId,
            int index, int start, int end)
        {
            var tempSlicedTensor = ReceivedMeasurement[torch.TensorIndex.Slice(start, end)];
            var scalarEvent = new ScalarResultEvent
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

        private Task PublishSummaryAsync(IAsyncStream<SummaryResultEvent> summaryResultEventStream, Guid storageId,
            int receivedSample, int sample, DateTime startTime, DateTime endTime)
        {
            var slicedTensor = ReceivedMeasurement[torch.TensorIndex.Slice(0, receivedSample)];
            var summaryEvent = new SummaryResultEvent
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
