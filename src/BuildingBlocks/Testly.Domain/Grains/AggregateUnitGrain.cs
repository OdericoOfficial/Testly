using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.States.Abstractions;
using TorchSharp;

namespace Testly.Domain.Grains
{ 
    internal sealed partial class AggregateUnitGrain
    {
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public async Task ExecuteAsync(int sample, int batchSize)
        {
            if (State.CurrentState != ScheduledNodeState.Executing)
            {
                await this.RegisterOrUpdateReminder(Constants.DefaultAggregateCompletedReminder,
                    TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));
                State.ApplyExecute(sample, batchSize);
            }
        }

        public Task OnNextAsync(AggregateUnitEvent item)
        {
            if (State.CurrentState == ScheduledNodeState.Executing)
            {
                var time = Convert.ToSingle((item.EndTime - item.StartTime).TotalMilliseconds);
                ReceivedMeasurement[State.ReceivedSample] = time;
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
            if (State.CurrentState == ScheduledNodeState.Executing)
            {
                var reminder = await this.GetReminder(Constants.DefaultAggregateCompletedReminder);
                if (reminder is not null)
                    await this.UnregisterReminder(reminder);
                State.ApplyCancelled();
            }
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == Constants.DefaultAggregateCompletedReminder
                && (DateTime.UtcNow - State.LastPublish) > TimeSpan.FromMinutes(2)
                && State.CurrentState == ScheduledNodeState.Executing)
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

            var reminder = await this.GetReminder(Constants.DefaultAggregateCompletedReminder);
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
                var scalarStream = StreamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, storageId);

                while (end < State.ReceivedSample)
                {
                    end = end + State.BatchSize < State.ReceivedSample ? end + State.BatchSize : State.ReceivedSample;

                    await PublishScalarAsync(scalarStream, storageId, 
                        start / State.BatchSize, start, end);

                    start += State.BatchSize;
                }

                var summaryStream = StreamProvider.GetStream<SummaryEvent>(Constants.DefaultSummaryNamespcace, storageId);
                await PublishSummaryAsync(summaryStream, storageId, 
                    State.ReceivedSample, State.Sample, State.StartTime, State.EndTime);
            }
        }

        private Task PublishScalarAsync(IAsyncStream<ScalarEvent> scalarStream, Guid storageId, 
            int index, int start, int end)
        {
            var tempSlicedTensor = ReceivedMeasurement[torch.TensorIndex.Slice(start, end)];
            var scalarEvent = new ScalarEvent
            {
                PublisherId = UnitId,
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

        private Task PublishSummaryAsync(IAsyncStream<SummaryEvent> summaryStream, Guid storageId, 
            int receivedSample, int sample, DateTime startTime, DateTime endTime)
        {
            var slicedTensor = ReceivedMeasurement[torch.TensorIndex.Slice(0, receivedSample)];
            var summaryEvent = new SummaryEvent
            {
                PublisherId = UnitId,
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
