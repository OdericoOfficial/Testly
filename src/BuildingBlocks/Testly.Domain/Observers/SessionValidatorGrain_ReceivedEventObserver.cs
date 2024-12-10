using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent>
    {
        private class ReceivedEventObserver : IAsyncObserver<TReceivedEvent>
        {
            private readonly SessionValidatorGrain<TSentEvent, TReceivedEvent> _grain;
            private readonly ILogger<ReceivedEventObserver> _logger;

            public ReceivedEventObserver(SessionValidatorGrain<TSentEvent, TReceivedEvent> grain, ILogger<ReceivedEventObserver> logger)
            {
                _grain = grain;
                _logger = logger;
            }

            public Task OnNextAsync(TReceivedEvent item, StreamSequenceToken? token = null)
            {
                _grain.State.ReceivedEvent = item;
                return Task.CompletedTask;
            }

            public Task OnErrorAsync(Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    typeof(TReceivedEvent).Name, _grain.GetType().Name, _grain.GetPrimaryKey());
                return Task.CompletedTask;
            }

            [Rougamo<LoggingException>]
            public Task OnCompletedAsync()
                => Interlocked.Increment(ref _grain._completedCount) == 2 ? _grain.PublishAsync() : Task.CompletedTask;
        }
    }
}
