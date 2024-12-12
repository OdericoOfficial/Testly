using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Testly.DependencyInjection.Attributes;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisteredService(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
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
