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
