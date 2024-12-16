using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>, Enumerable]
    internal class ThrowExceptionMockMethodValueSimple : IThrowExceptionValueMock
    {
        private readonly ILogger _logger;

        public ThrowExceptionMockMethodValueSimple(ILogger<ThrowExceptionMockMethodValueSimple> logger)
            => _logger = logger;

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionValue<int>>]
#else
        [LoggingExceptionValue<int>]
#endif
        public int NotThrow()
            => 0;

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionValue<int>>]
#else
        [LoggingExceptionValue<int>]
#endif
        public async Task<int> NotThrowAsync()
        {
            await Task.Delay(100);
            return 0;
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionValue<int>>]
#else
        [LoggingExceptionValue<int>]
#endif
        public int ThrowException()
            => throw new NotImplementedException();

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionValue<int>>]
#else
        [LoggingExceptionValue<int>]
#endif
        public async Task<int> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}