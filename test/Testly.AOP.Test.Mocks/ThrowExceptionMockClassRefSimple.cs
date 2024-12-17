using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>, Enumerable]
    internal class ThrowExceptionMockClassRefSimple : IThrowExceptionRefMock,
        IRougamo<LoggingExceptionRef<object>>

    {
        private readonly ILogger _logger;

        public ThrowExceptionMockClassRefSimple(ILogger<ThrowExceptionMockClassRefSimple> logger)
            => _logger = logger;

        public object NotThrow()
            => new object();

        public async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }

        public object ThrowException()
            => throw new NotImplementedException();

        public async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}