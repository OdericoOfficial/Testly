using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduleUnitGrain<TSentEvent, TRequest, TCommand>
    {
        private class ScalarEventObserver : IAsyncObserver<ScalarEvent>
        {
            private readonly ScheduleUnitGrain<TSentEvent, TRequest, TCommand> _grain;
            private readonly ILogger<ScalarEventObserver> _logger;

            public ScalarEventObserver(ScheduleUnitGrain<TSentEvent, TRequest, TCommand> grain, ILogger<ScalarEventObserver> logger)
            {
                _grain = grain;
                _logger = logger;
            }

            public Task OnNextAsync(ScalarEvent item, StreamSequenceToken? token = null)
            {
                if (item.AggregateId == _grain.State.CurrentAggregateId)
                    _grain.State.Scalars.Add(item);
                return Task.CompletedTask;
            }

            public Task OnErrorAsync(Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    nameof(ScalarEvent), _grain.GetType().Name, _grain.GetPrimaryKey());
                return Task.CompletedTask;
            }

            [Rougamo<LoggingException>]
            public Task OnCompletedAsync()
                => Interlocked.Increment(ref _grain._completedCount) == 2 ?  _grain.OnAggregateCompletedAsync() : Task.CompletedTask;
        }
    }
}
