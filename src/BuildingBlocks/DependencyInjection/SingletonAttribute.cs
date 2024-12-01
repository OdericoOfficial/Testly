using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingletonAttribute : ServiceAttribute
    {
        public override ServiceLifetime ServiceLifetime 
            => ServiceLifetime.Singleton;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingletonAttribute<TService> : SingletonAttribute
    {
        public override Type? ServiceType { get; set; } = typeof(TService);
    }
}
