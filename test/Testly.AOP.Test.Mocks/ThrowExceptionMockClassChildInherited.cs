using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>]
    internal class ThrowExceptionMockClassChildInherited : ThrowExceptionMockChildAbstract,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
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
