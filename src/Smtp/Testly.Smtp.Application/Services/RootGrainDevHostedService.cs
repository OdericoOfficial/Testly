using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Smtp.Application.Services
{
#if DEBUG
    [HostedService]
#endif
    internal class RootGrainDevHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IGuidFactory _factory;
        private readonly IClusterClient _clusterClient;

        public RootGrainDevHostedService(ILogger<RootGrainDevHostedService> logger, IGuidFactory factory, IClusterClient clusterClient)
        {
            _logger = logger;
            _factory = factory;
            _clusterClient = clusterClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogCritical("Debug start.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
