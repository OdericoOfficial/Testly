using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>, Enumerable]
    internal class ThrowExceptionMockMethodChildInherited : ThrowExceptionMockChildAbstract
    {
        public ThrowExceptionMockMethodChildInherited(ILogger<ThrowExceptionMockMethodChildInherited> logger) : base(logger)
        {
        }


        [Rougamo<LoggingException>]

        

        public override void NotThrow()
        {
        }


        [Rougamo<LoggingException>]

        

        public override async Task NotThrowAsync()
            => await Task.Delay(100);


        [Rougamo<LoggingException>]

        

        public override void ThrowException()
        {
            throw new NotImplementedException();
        }


        [Rougamo<LoggingException>]

        

        public override async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}