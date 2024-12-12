using Microsoft.Extensions.Logging;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockChildAbstract : IThrowExceptionMock
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockChildAbstract(ILogger logger)
            => _logger = logger;

        public abstract void NotThrow();

        public abstract Task NotThrowAsync();
        
        public abstract void ThrowException();
        
        public abstract Task ThrowExceptionAsync();
    }
}
