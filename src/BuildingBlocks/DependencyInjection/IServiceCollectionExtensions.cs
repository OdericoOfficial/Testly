using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace BuildingBlocks.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisteredService(this IServiceCollection services)
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()))
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>();
                if (attribute is not null)
                    services.TryAddEnumerable(new ServiceDescriptor(attribute.ServiceType is not null ? attribute.ServiceType : type,
                        attribute.Key, type, attribute.ServiceLifetime));
            }
            return services;
        }
    }
}
