using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public abstract class ServiceAttribute : Attribute
    {
        public abstract ServiceLifetime ServiceLifetime { get; }

        public string? Key { get; set; }

        public virtual Type? ServiceType { get; set; }
    }
}
