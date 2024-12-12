using Microsoft.Extensions.Logging;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockChildValueAbstract : IThrowExceptionValueMock
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockChildValueAbstract(ILogger logger)
            => _logger = logger;

        public abstract int NotThrow();

        public abstract Task<int> NotThrowAsync();
        
        public abstract int ThrowException();
        
        public abstract Task<int> ThrowExceptionAsync();
    }
}
