using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Testly.AOP.Tests
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services) 
            => services.AddLogging(builder => builder.AddXunitOutput())
                .AddMocks();
    }
}
