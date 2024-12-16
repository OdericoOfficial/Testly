using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testly.Domain.Factories;
using Testly.Domain.Factories.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
            => services.AddMapster()
                .AddMarkedServices();

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
            services.TryAddSingleton(typeof(ISchduleSentEventFactory<,>), typeof(MapsterScheduleSentEventFactory<,>));
            services.TryAddSingleton(typeof(ISchduleReceivedEventFactory<,,>), typeof(MapsterScheduleReceivedEventFactory<,,>));
            return services;
        }
    }
}
