using Microsoft.Extensions.DependencyInjection.Extensions;
using Testly.Domain.Factories;
using Testly.Domain.Factories.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainBase(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(ISentEventFactory<,>), typeof(MapsterSentEventFactory<,>));
            services.TryAddSingleton(typeof(IReceivedEventFactory<,,>), typeof(MapsterReceivedEventFactory<,,>));
            return services.AddMarkedServices();
        }
    }
}
