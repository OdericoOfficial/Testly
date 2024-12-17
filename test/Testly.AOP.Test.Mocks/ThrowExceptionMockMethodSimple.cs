using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>, Enumerable]
    internal class ThrowExceptionMockMethodSimple : IThrowExceptionMock
    {
        private readonly ILogger _logger;

        public ThrowExceptionMockMethodSimple(ILogger<ThrowExceptionMockMethodSimple> logger)
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
            => throw new NotImplementedException();


        [Rougamo<LoggingException>]

        

        public async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}