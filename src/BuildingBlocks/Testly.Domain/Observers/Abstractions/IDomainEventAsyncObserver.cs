using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Observers.Abstractions
{
    public interface IDomainEventAsyncObserver<TDomainEvent> : IGrainWithGuidKey
        where TDomainEvent : IDomainEvent
    {
        Task OnNextAsync(TDomainEvent item);
    }
}
