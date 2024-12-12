using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>]
    internal class ThrowExceptionMockClassInherited : ThrowExceptionMockClassAbstract
    {
        public ThrowExceptionMockClassInherited(ILogger<ThrowExceptionMockClassInherited> logger) : base(logger)
        {
        }
    }
}
