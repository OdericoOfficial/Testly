using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>]
    internal class ThrowExceptionMockClassRefSimple : IThrowExceptionRefMock, 
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingExceptionRef<object>>
#else
        IRougamo<LoggingExceptionRefAttribute<object>>
#endif
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
