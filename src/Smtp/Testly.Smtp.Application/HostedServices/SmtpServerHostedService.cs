using Microsoft.Extensions.Hosting;
using Testly.DependencyInjection;
using Server = SmtpServer.SmtpServer;


namespace Testly.Smtp.Application.HostedServices
{
    [HostedService]
    internal class SmtpServerHostedService : BackgroundService
    {
        private readonly Server _server;

        public SmtpServerHostedService(Server server)
            => _server = server;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.StartAsync(stoppingToken);

        }
    }
}
