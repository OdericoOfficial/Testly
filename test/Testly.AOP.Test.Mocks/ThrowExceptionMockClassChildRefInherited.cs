using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>, Enumerable]
    internal class ThrowExceptionMockClassChildRefInherited : ThrowExceptionMockChildRefAbstract,
        IRougamo<LoggingExceptionRef<object>>
    {
        public ThrowExceptionMockClassChildRefInherited(ILogger<ThrowExceptionMockClassChildRefInherited> logger) : base(logger)
        {
        }

        public override object NotThrow()
            => new object();

        public override async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }

        public override object ThrowException()
            => throw new NotImplementedException();

        public override async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}