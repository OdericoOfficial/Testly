using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockClassValueAbstract : IThrowExceptionValueMock, 
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingExceptionValue<int>>
#else
        IRougamo<LoggingExceptionValueAttribute<int>>
#endif
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockClassValueAbstract(ILogger logger)
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
