using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>]
    internal class ThrowExceptionMockClassRefInherited : ThrowExceptionMockClassRefAbstract
    {
        public ThrowExceptionMockClassRefInherited(ILogger<ThrowExceptionMockClassRefInherited> logger) : base(logger)
        {
        }
    }
}
