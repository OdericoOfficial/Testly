using System.Security.Cryptography.X509Certificates;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmtpServer;
using Server = SmtpServer.SmtpServer;

namespace Testly.Smtp.Application
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationModule(this IServiceCollection services)
        {
            static X509Certificate2 CreateCertificate()
            {
                var certificate = File.ReadAllBytes(@"server.pfx");
                return new X509Certificate2(certificate, "1234");
            }

            services.TryAddSingleton(provier =>
                new Server(new SmtpServerOptionsBuilder()
                    .ServerName("localhost")
                    .Endpoint(builder =>
                        builder.Port(25, true)
                            .AllowUnsecureAuthentication(false)
                            .Certificate(CreateCertificate()))
                    .Build(), provier));

            services.TryAddSingleton(provider =>
            {
                var config = new TypeAdapterConfig();
                foreach (var register in provider.GetServices<IRegister>())
                    register.Register(config);
                return config;
            });
            services.TryAddScoped<IMapper, ServiceMapper>();

            return services;
        }
    }
}
