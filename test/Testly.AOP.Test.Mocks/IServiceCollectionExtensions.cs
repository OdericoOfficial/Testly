using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMocks(this IServiceCollection services)
            => services.AddRegisteredService(Assembly.GetExecutingAssembly());
    }
}
