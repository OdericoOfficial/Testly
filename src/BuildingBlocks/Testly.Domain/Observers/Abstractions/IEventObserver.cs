using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Observers.Abstractions
{
    public interface IEventObserver<TEvent> : IGrainWithGuidKey
        where TEvent : IEvent
    {
        Task OnNextAsync(TEvent item);
    }
}
