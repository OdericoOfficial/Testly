using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockClassAbstract : IThrowExceptionMock,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockClassAbstract(ILogger logger)
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
