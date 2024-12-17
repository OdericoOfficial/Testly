using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>, Enumerable]
    internal class ThrowExceptionMockMethodChildRefInherited : ThrowExceptionMockChildRefAbstract
    {
        public ThrowExceptionMockMethodChildRefInherited(ILogger<ThrowExceptionMockMethodChildRefInherited> logger) : base(logger)
        {
        }

        [Rougamo<LoggingExceptionRef<object>>]
        public override object NotThrow()
            => new object();

        [Rougamo<LoggingExceptionRef<object>>]
        public override async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }

        [Rougamo<LoggingExceptionRef<object>>]
        public override object ThrowException()
            => throw new NotImplementedException();

        [Rougamo<LoggingExceptionRef<object>>]
        public override async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}