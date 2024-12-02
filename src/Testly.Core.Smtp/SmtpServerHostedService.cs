using Microsoft.Extensions.Hosting;
using Testly.DependencyInjection;
using Server = SmtpServer.SmtpServer;

namespace Testly.Core.Smtp
{
    [HostedService]
    internal class SmtpServerHostedService : IHostedService
    {
        private readonly Server _smtpServer;

        public SmtpServerHostedService(Server smtpServer)
            => _smtpServer = smtpServer;

        public Task StartAsync(CancellationToken cancellationToken)
            => _smtpServer.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
