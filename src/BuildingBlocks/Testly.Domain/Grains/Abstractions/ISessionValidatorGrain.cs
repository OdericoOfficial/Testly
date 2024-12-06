using Orleans.Streams;
using Testly.Domain.Events;

namespace Testly.Domain.Grains.Abstractions
{
    public interface ISessionValidatorGrain<TSentEvent, TReceivedEvent> : IGrainWithGuidKey
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        Task OnSentEventReceivedAsync(TSentEvent sentEvent, StreamSequenceToken? sequenceToken = null);

        Task OnReceivedEventReceivedAsync(TReceivedEvent receivedEvent, StreamSequenceToken? sequenceToken = null);
    }
}
