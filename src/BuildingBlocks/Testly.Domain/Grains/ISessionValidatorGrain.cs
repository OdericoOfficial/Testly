using Orleans;

namespace Testly.Domain.Grains
{
    public interface ISessionValidatorGrain<TSentEvent, TReceivedEvent> : IGrainWithStringKey
    {
        Task CacheSentEventAsync(TSentEvent sentEvent);
        
        Task CacheReceivedEventAsync(TReceivedEvent receivedEvent);
    }
}
