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


        [Rougamo<LoggingExceptionValue<int>>]

        

        public int NotThrow()
            => 0;


        [Rougamo<LoggingExceptionValue<int>>]

        

        public async Task<int> NotThrowAsync()
        {
            await Task.Delay(100);
            return 0;
        }


        [Rougamo<LoggingExceptionValue<int>>]

        

        public int ThrowException()
            => throw new NotImplementedException();


        [Rougamo<LoggingExceptionValue<int>>]

        

        public async Task<int> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
