using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>, Enumerable]
    internal class ThrowExceptionMockClassChildInherited : ThrowExceptionMockChildAbstract,

        IRougamo<LoggingException>

        

    {
        public ThrowExceptionMockClassChildInherited(ILogger<ThrowExceptionMockClassChildInherited> logger) : base(logger)
        {
        }

        public override void NotThrow()
        {
        }

        public override async Task NotThrowAsync()
            => await Task.Delay(100);

        public override void ThrowException()
            => throw new NotImplementedException();

        public override async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}