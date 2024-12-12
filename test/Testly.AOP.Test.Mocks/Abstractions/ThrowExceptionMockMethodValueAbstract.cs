using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockMethodValueAbstract : IThrowExceptionValueMock
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockMethodValueAbstract(ILogger logger)
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
