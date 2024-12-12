using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>]
    internal class ThrowExceptionMockMethodSimple : IThrowExceptionMock
    {
        private readonly ILogger _logger;

        public ThrowExceptionMockMethodSimple(ILogger<ThrowExceptionMockMethodSimple> logger) 
            => _logger = logger;

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public void NotThrow()
        {
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public async Task NotThrowAsync()
            => await Task.Delay(100);

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public void ThrowException()
            => throw new NotImplementedException();

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);  
            throw new NotImplementedException();
        }
    }
}
