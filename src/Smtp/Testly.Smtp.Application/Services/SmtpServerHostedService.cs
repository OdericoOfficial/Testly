using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.Smtp.Application.Services
{
    [HostedService]
    internal class SmtpServerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly SmtpServer.SmtpServer _server;

        public SmtpServerHostedService(ILogger<SmtpServerHostedService> logger, SmtpServer.SmtpServer server)
        {
            _logger = logger;
            _server = server;
        }

        [Rougamo<LoggingException>]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.StartAsync(stoppingToken);
            _logger.LogCritical("SmtpServer Started.");
        }
    }
}
