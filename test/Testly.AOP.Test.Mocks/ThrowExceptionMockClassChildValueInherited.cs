using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>]
    internal class ThrowExceptionMockClassChildValueInherited : ThrowExceptionMockChildValueAbstract, 
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingExceptionValue<int>>
#else
        IRougamo<LoggingExceptionValueAttribute<int>>
#endif
    {
        public ThrowExceptionMockClassChildValueInherited(ILogger<ThrowExceptionMockClassChildValueInherited> logger) : base(logger)
        {
        }

        public override int NotThrow()
            => 0;

        public override async Task<int> NotThrowAsync()
        {
            await Task.Delay(100);
            return 0;
        }

        public override int ThrowException()
            => throw new NotImplementedException();

        public override async Task<int> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
