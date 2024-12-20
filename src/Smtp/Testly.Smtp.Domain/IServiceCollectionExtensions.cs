using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using MimeKit;
using Testly.Domain.Attributes;
using Testly.Smtp.Domain.Events;

namespace Microsoft.Extensions.DependencyInjection
{
    //[Duplex<SmtpSentEvent, SmtpReceivedEvent, MimeMessage>]
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
            => services.AddMarkedServices()
                .AddDomainBase()
                .AddSmtpClientPool();

        internal static IServiceCollection AddSmtpClientPool(this IServiceCollection services)
        {
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton(provider =>
            {
                var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
                var policy = new DefaultPooledObjectPolicy<SmtpClient>();
                return poolProvider.Create(policy);
            });
            return services;
        }
    }
}
