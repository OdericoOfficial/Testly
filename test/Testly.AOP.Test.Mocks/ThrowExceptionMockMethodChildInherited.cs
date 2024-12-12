using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>]
    internal class ThrowExceptionMockMethodChildInherited : ThrowExceptionMockChildAbstract
    {
        public ThrowExceptionMockMethodChildInherited(ILogger<ThrowExceptionMockMethodChildInherited> logger) : base(logger) { }


#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override void NotThrow()
        {
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task NotThrowAsync()
            => await Task.Delay(100);

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override void ThrowException()
        {
            throw new NotImplementedException();
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingException>]
#else
        [LoggingException]
#endif
        public override async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
