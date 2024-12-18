using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testly.Domain.Factories;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies;
using Testly.Domain.Policies.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
            => services.AddPolicies()
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
            services.TryAddSingleton(typeof(ISentEventFactory<,>), typeof(MapsterSentEventFactory<,>));
            services.TryAddSingleton(typeof(IReceivedEventFactory<,,>), typeof(MapsterReceivedEventFactory<,,>));
            return services;
        }

        internal static IServiceCollection AddPolicies(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(IIncreasePolicy<>), typeof(IncreasePolicy<>));
            services.TryAddSingleton(typeof(ISpikesPolicy<>), typeof(SpikesPolicy<>));
            services.TryAddSingleton(typeof(ISerialPolicy<>), typeof(SerialPolicy<>));
            return services;
        }
    }
}
