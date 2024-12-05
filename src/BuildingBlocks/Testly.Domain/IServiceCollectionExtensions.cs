using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testly.Domain.Events;
using Testly.Domain.Factories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMapster(this IServiceCollection services)
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

        public static IServiceCollection AddMapsterScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse>(this IServiceCollection services)
            where TSentEvent : SentEvent
            where TReceivedEvent : ReceivedEvent
        {
            services.TryAddSingleton<IScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse>, MapsterScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse>>();
            return services;
        }
    }
}
