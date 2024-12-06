using Microsoft.Extensions.DependencyInjection;

namespace Testly.DependencyInjection.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScopedAttribute : ServiceAttribute
    {
        public override ServiceLifetime ServiceLifetime
            => ServiceLifetime.Scoped;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScopedAttribute<TService> : SingletonAttribute
    {
        public override Type? ServiceType { get; set; } = typeof(TService);
    }
}
