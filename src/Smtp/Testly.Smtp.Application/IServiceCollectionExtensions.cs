using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmtpServer;
using TorchSharp;
using TorchSharp.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicaiton(this IServiceCollection services)
            => services.AddDomain()
                .AddMarkedServices()
                .AddMapster()
                .AddSmtpServer();
        
        internal static IServiceCollection AddMapster(this IServiceCollection services)
        {
            services.TryAddSingleton(provider =>
            {
                var config = new TypeAdapterConfig();
                var registers = provider.GetServices<IRegister>();
                foreach (var item in registers)
                    item.Register(config);
                return config;
            });
            services.TryAddSingleton<IMapper, ServiceMapper>();
            return services;
        }

        internal static IServiceCollection AddSmtpServer(this IServiceCollection services)
        {
            services.TryAddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var options = new SmtpServerOptionsBuilder()
                    .ServerName(configuration["SmtpServer:ReceiverName"])
                    .Port(Convert.ToInt32(configuration["SmtpServer:ReceiverPort"]))
                    .Build();

                return new SmtpServer.SmtpServer(options, provider);
            });
            return services;
        }
    
        internal static IServiceCollection AddSummaryWriter(this IServiceCollection services)
        {
            services.TryAddSingleton(provider =>
                torch.utils.tensorboard.SummaryWriter("./data"));
            return services;
        }
    }
}
