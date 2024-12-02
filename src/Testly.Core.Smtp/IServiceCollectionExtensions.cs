using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmtpServer;
using Server = SmtpServer.SmtpServer;

namespace Testly.Core.Smtp
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSmtpServer(this IServiceCollection services, Action<SmtpServerOptionsBuilder>? configureOptions = null)
        {
            services.TryAddSingleton(provider =>
            {
                var smtpServerBuilder = new SmtpServerOptionsBuilder();
                configureOptions?.Invoke(smtpServerBuilder);
                var options = smtpServerBuilder.Build();
                return new Server(options, provider);
            });
            return services;
        }
    }
}
