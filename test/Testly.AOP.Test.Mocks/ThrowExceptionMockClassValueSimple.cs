using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>]
    internal class ThrowExceptionMockClassValueSimple : IThrowExceptionValueMock,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingExceptionValue<int>>
#else
        IRougamo<LoggingExceptionValueAttribute<int>>
#endif
    {
        private readonly ILogger _logger;

        public ThrowExceptionMockClassValueSimple(ILogger<ThrowExceptionMockClassValueSimple> logger)
            => _logger = logger;

        public int NotThrow()
            => 0;

        public async Task<int> NotThrowAsync()
        {
            await Task.Delay(100);
            return 0;
        }

        public int ThrowException()
            => throw new NotImplementedException();

        public async Task<int> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
