using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;

namespace Testly.Domain.Grains
{
    internal partial class AggregateGrain
    {
        private class AggregateEventObserver : IAsyncObserver<AggregateEvent>
        {
            private readonly AggregateGrain _grain;
            private readonly ILogger<AggregateEventObserver> _logger;

            public AggregateEventObserver(AggregateGrain grain, ILogger<AggregateEventObserver> logger)
            {
                _grain = grain;
                _logger = logger;
            }

            public Task OnNextAsync(AggregateEvent item, StreamSequenceToken? token = null)
            {
                var time = Convert.ToSingle((item.ReceivedTime - item.SendingTime).TotalMilliseconds);
                _grain.State.ReceivedSample++;

                if (_grain._receivedMeasurement is not null)
                    _grain._receivedMeasurement[item.ReceivedIndex] = time;

                if (_grain.State.StartTime > item.SendingTime)
                    _grain.State.StartTime = item.SendingTime;

                if (_grain.State.EndTime < item.ReceivedTime)
                    _grain.State.EndTime = item.ReceivedTime;

                _grain.State.LastPublish = DateTime.UtcNow;

                return Task.CompletedTask;
            }

            public Task OnErrorAsync(Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    nameof(AggregateEvent), nameof(AggregateGrain), _grain.GetPrimaryKey());
                return Task.CompletedTask;
            }

            [Rougamo<LoggingException>]
            public async Task OnCompletedAsync()
            {
                await _grain.PublishAsync();
                var reminder = await _grain.GetReminder(Constants.DefaultAggregateCompleteReminder);
                if (reminder is not null)
                    await _grain.UnregisterReminder(reminder);

                if (_grain._receivedMeasurement is not null)
                {
                    _grain._receivedMeasurement.Dispose();
                    _grain._receivedMeasurement = null;
                }

                await _grain.ClearStateAsync();
            }
        }
    }
}
