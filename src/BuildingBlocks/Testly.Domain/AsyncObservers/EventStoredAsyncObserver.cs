using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Repositories;

namespace Testly.Domain.AsyncObservers
{
    internal class EventStoredAsyncObserver<TDomainEvent> : IAsyncObserver<TDomainEvent>
    {
        private readonly IWritableRepository<TDomainEvent> _repository;
        private readonly ILogger<EventStoredAsyncObserver<TDomainEvent>> _logger;

        public EventStoredAsyncObserver(IWritableRepository<TDomainEvent> repository, ILogger<EventStoredAsyncObserver<TDomainEvent>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task OnNextAsync(TDomainEvent item, StreamSequenceToken? token = null)
            => _repository.AddAsync(item);

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex.ToString());
            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
            => Task.CompletedTask;
    }
}
