using InfluxDB.Client;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using InfluxDB.Client.Api.Domain;
using Testly.DependencyInjection;

namespace Testly.Core
{
    [Singleton(ServiceType = typeof(IAsyncObserver<>))]
    internal class InfluxDomainEventObserver<TDomainEvent> : IAsyncObserver<TDomainEvent>
    {
        private readonly IWriteApiAsync _writeApiAsync;
        private readonly ILogger<InfluxDomainEventObserver<TDomainEvent>> _logger;

        public InfluxDomainEventObserver(IWriteApiAsync writeApiAsync, ILogger<InfluxDomainEventObserver<TDomainEvent>> logger)
        {
            _writeApiAsync = writeApiAsync;
            _logger = logger;
        }

        public Task OnNextAsync(TDomainEvent domainEvent, StreamSequenceToken? token)
            => Task.CompletedTask;

        public Task OnCompletedAsync()
            => Task.CompletedTask;

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex.ToString());
            return Task.CompletedTask;
        }
    }
}
