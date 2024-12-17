using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>, Enumerable]
    internal class ThrowExceptionMockMethodRefSimple : IThrowExceptionRefMock
    {
        private readonly ILogger _logger;

        public ThrowExceptionMockMethodRefSimple(ILogger<ThrowExceptionMockMethodRefSimple> logger)
            => _logger = logger;

        [Rougamo<LoggingExceptionRef<object>>]
        public object NotThrow()
            => new object();

        [Rougamo<LoggingExceptionRef<object>>]
        public async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }

        [Rougamo<LoggingExceptionRef<object>>]
        public object ThrowException()
            => throw new NotImplementedException();

        [Rougamo<LoggingExceptionRef<object>>]
        public async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}