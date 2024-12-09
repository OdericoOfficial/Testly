using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories;
using Testly.Domain.Factories.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMapster(this IServiceCollection services)
        {
            var interfaceType = typeof(IRegister);
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()))
            {
                if (interfaceType.IsAssignableFrom(type))
                    services.TryAddSingleton(interfaceType, type);
            }

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

        public static IServiceCollection AddMapsterSentEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse>(this IServiceCollection services)
            where TSentEvent : struct, ISentEvent
            where TReceivedEvent : struct, IReceivedEvent
        {
            services.TryAddSingleton<ISchduleSentEventFactory<TSentEvent, TRequest>, MapsterScheduleSentEventFactory<TSentEvent, TRequest>>();
            return services;
        }

        public static IServiceCollection AddMapsterReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>(this IServiceCollection services)
            where TSentEvent : struct, ISentEvent
            where TReceivedEvent : struct, IReceivedEvent
        {
            services.TryAddSingleton<ISchduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>, MapsterScheduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>>();
            return services;
        }

        public static IServiceCollection AddLocalGuidFactory(this IServiceCollection services)
        {
            services.TryAddSingleton<IGuidFactory, LocalSequentialGuidFactory>();
            return services;
        }
    }
}
