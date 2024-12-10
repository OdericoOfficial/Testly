using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent>
    {
        private class SentEventObserver : IAsyncObserver<TSentEvent>
        {
            private readonly SessionValidatorGrain<TSentEvent, TReceivedEvent> _grain;
            private readonly ILogger<SentEventObserver> _logger;

            public SentEventObserver(SessionValidatorGrain<TSentEvent, TReceivedEvent> grain, ILogger<SentEventObserver> logger)
            {
                _grain = grain;
                _logger = logger;
            }

            public Task OnNextAsync(TSentEvent item, StreamSequenceToken? token = null)
            {
                _grain.State.SentEvent = item;
                return Task.CompletedTask;
            }

            public Task OnErrorAsync(Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    typeof(TSentEvent).Name, _grain.GetType().Name, _grain.GetPrimaryKey());
                return Task.CompletedTask;
            }

            [Rougamo<LoggingException>]
            public Task OnCompletedAsync()
                => Interlocked.Increment(ref _grain._completedCount) == 2 ? _grain.PublishAsync() : Task.CompletedTask;
        }
    }
}
