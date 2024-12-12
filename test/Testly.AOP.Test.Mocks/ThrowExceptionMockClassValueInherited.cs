using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>]
    internal class ThrowExceptionMockClassValueInherited : ThrowExceptionMockClassValueAbstract
    {
        public ThrowExceptionMockClassValueInherited(ILogger<ThrowExceptionMockClassValueInherited> logger) : base(logger)
        {
        }
    }
}
