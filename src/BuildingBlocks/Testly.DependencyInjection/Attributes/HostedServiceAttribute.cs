using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Testly.DependencyInjection.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HostedServiceAttribute : ServiceAttribute
    {
        public override ServiceLifetime ServiceLifetime
            => ServiceLifetime.Singleton;

        public override Type? ServiceType { get; set; } = typeof(IHostedService);
    }
}
