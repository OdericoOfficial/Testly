using Testly.Domain.Events;

namespace Testly.Domain.Grains
{
    public interface ISessionValidatorGrain<TSentEvent, TReceivedEvent> : IGrainWithStringKey
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        Task CacheSentEventAsync(TSentEvent sentEvent);
        
        Task CacheReceivedEventAsync(TReceivedEvent receivedEvent);
    }
}
