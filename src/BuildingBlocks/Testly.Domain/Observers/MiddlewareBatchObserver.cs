using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;

namespace Testly.Domain.Observers
{
    internal class MiddlewareBatchObserver<TEvent> : IAsyncBatchObserver<TEvent>
        where TEvent : IBatchEvent
    {
        private readonly ILogger<MiddlewareBatchObserver<TEvent>> _logger;
        private readonly IClusterClient _clusterClient;

        public MiddlewareBatchObserver(ILogger<MiddlewareBatchObserver<TEvent>> logger, IClusterClient clusterClient)
        {
            _logger = logger;
            _clusterClient = clusterClient;
        }

        public Task OnNextAsync(IList<SequentialItem<TEvent>> items)
        {
            if (items.Any())
            {
                var item = items.First();
                if (item.Item.Mode == BatchMode.Parallel)
                {
                    OnNextParallel(items);
                    return Task.CompletedTask;
                }
                else
                    return OnNextSerialAsync(items);
            }

            return Task.CompletedTask;
        }

        private void OnNextParallel(IList<SequentialItem<TEvent>> items)
        {
            foreach (var item in items)
                _clusterClient.GetGrain<IEventObserver<TEvent>>(item.Item.SubscriberId).OnNextAsync(item.Item);
        }

        private async Task OnNextSerialAsync(IList<SequentialItem<TEvent>> items)
        {
            foreach (var item in items)
                await _clusterClient.GetGrain<IEventObserver<TEvent>>(item.Item.SubscriberId).OnNextAsync(item.Item);
        }

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
