namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMocks(this IServiceCollection services)
            => services.AddMarkedServices();
    }
}
