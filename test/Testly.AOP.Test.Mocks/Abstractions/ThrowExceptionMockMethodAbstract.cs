using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockMethodAbstract : IThrowExceptionMock
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockMethodAbstract(ILogger logger)
            => _logger = logger;


        [Rougamo<LoggingException>]

        

        public void NotThrow()
        {
        }


        [Rougamo<LoggingException>]

        

        public async Task NotThrowAsync()
            => await Task.Delay(100);


        [Rougamo<LoggingException>]

        

        public void ThrowException()
        {
            throw new NotImplementedException();
        }


        [Rougamo<LoggingException>]

        

        public async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
