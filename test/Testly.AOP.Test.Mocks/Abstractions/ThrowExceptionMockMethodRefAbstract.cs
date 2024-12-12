using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockMethodRefAbstract : IThrowExceptionRefMock
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockMethodRefAbstract(ILogger logger)
            => _logger = logger;

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public object NotThrow()
            => new object();

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }


#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public object ThrowException()
            => throw new NotImplementedException();

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else   
        [LoggingExceptionRef<object>]
#endif
        public async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
