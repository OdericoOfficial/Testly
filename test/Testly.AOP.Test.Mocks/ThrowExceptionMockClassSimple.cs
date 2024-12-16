using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>, Enumerable]
    internal class ThrowExceptionMockClassSimple : IThrowExceptionMock,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
    {
        private readonly ILogger _logger;

        public ThrowExceptionMockClassSimple(ILogger<ThrowExceptionMockClassSimple> logger)
            => _logger = logger;

        public void NotThrow()
        {
        }

        public async Task NotThrowAsync()
            => await Task.Delay(100);

        public void ThrowException()
            => throw new NotImplementedException();

        public async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}