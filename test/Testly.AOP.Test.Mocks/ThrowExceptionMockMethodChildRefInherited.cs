using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>]
    internal class ThrowExceptionMockMethodChildRefInherited : ThrowExceptionMockChildRefAbstract
    {
        public ThrowExceptionMockMethodChildRefInherited(ILogger<ThrowExceptionMockMethodChildRefInherited> logger) : base(logger) { }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public override object NotThrow()
            => new object();

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public override async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public override object ThrowException()
            => throw new NotImplementedException();

#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        [Rougamo<LoggingExceptionRef<object>>]
#else
        [LoggingExceptionRef<object>]
#endif
        public override async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
