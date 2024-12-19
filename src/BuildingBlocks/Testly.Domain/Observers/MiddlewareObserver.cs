using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;

namespace Testly.Domain.Observers
{
    internal sealed class MiddlewareObserver<TEvent> : IAsyncObserver<TEvent>
        where TEvent : IEvent
    {
        private readonly ILogger<MiddlewareObserver<TEvent>> _logger;
        private readonly IClusterClient _clusterClient;
        
        public MiddlewareObserver(IClusterClient clusterClient, ILogger<MiddlewareObserver<TEvent>> logger)
        {
            _clusterClient = clusterClient;
            _logger = logger;
        }

        public Task OnNextAsync(TEvent item, StreamSequenceToken? token = null)
            => _clusterClient.GetGrain<IEventObserver<TEvent>>(item.SubscriberId)
                .OnNextAsync(item);

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception in AsyncStream {EventName} from {ObserverName}",
                typeof(TEvent).Name, GetType().Name);
            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
            => Task.CompletedTask;
    }
}
