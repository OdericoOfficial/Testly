using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.Streams;
using Testly.Domain.Factories;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Observers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainBase(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(ISentEventFactory<,>), typeof(MapsterSentEventFactory<,>));
            services.TryAddSingleton(typeof(IReceivedEventFactory<,,>), typeof(MapsterReceivedEventFactory<,,>));
            services.TryAddSingleton(typeof(IAsyncObserver<>), typeof(MiddlewareObserver<>));
            services.TryAddSingleton(typeof(IAsyncBatchObserver<>), typeof(MiddlewareBatchObserver<>));
            return services.AddMarkedServices();
        }
    }
}
