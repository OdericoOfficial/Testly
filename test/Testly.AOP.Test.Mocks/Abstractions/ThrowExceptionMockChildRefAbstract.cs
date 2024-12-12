using Microsoft.Extensions.Logging;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockChildRefAbstract : IThrowExceptionRefMock
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockChildRefAbstract(ILogger logger)
            => _logger = logger;

        public abstract object NotThrow();

        public abstract Task<object> NotThrowAsync();
        
        public abstract object ThrowException();
        
        public abstract Task<object> ThrowExceptionAsync();
    }
}
