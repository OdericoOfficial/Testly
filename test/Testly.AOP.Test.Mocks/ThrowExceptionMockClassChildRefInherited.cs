using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>]
    internal class ThrowExceptionMockClassChildRefInherited : ThrowExceptionMockChildRefAbstract,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingExceptionRef<object>>
#else
        IRougamo<LoggingExceptionRefAttribute<object>>
#endif
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
