using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>]
    internal class ThrowExceptionMockMethodRefInherited : ThrowExceptionMockMethodRefAbstract
    {
        public ThrowExceptionMockMethodRefInherited(ILogger<ThrowExceptionMockMethodRefInherited> logger) : base(logger)
        {
        }
    }
}
