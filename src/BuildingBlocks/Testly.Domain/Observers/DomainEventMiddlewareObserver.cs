using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;

namespace Testly.Domain.Observers
{
    internal class DomainEventMiddlewareObserver<TDomainEvent> : IAsyncObserver<TDomainEvent>
        where TDomainEvent : IDomainEvent
    {
        private readonly ILogger<DomainEventMiddlewareObserver<TDomainEvent>> _logger;
        private readonly IClusterClient _clusterClient;
        
        public DomainEventMiddlewareObserver(IClusterClient clusterClient, ILogger<DomainEventMiddlewareObserver<TDomainEvent>> logger)
        {
            _clusterClient = clusterClient;
            _logger = logger;
        }

        public Task OnNextAsync(TDomainEvent item, StreamSequenceToken? token = null)
        {
            var grain = _clusterClient.GetGrain<IDomainEventAsyncObserver<TDomainEvent>>(item.SubscriberId);
            return grain.OnNextAsync(item);
        }

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception in AsyncStream {EventName} from {ObserverName}",
                typeof(TDomainEvent).Name, GetType().Name);
            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
            => Task.CompletedTask;
    }
}
