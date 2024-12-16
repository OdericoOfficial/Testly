using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Observers.Abstractions
{
    public interface IDomainEventAsyncObserver<TDomainEvent> : IGrainWithGuidKey
        where TDomainEvent : DomainEvent
    {
        Task OnNextAsync(TDomainEvent item);
    }
}
