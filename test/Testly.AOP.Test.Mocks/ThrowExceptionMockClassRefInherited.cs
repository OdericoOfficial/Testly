using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>, Enumerable]
    internal class ThrowExceptionMockClassRefInherited : ThrowExceptionMockClassRefAbstract
    {
        public ThrowExceptionMockClassRefInherited(ILogger<ThrowExceptionMockClassRefInherited> logger) : base(logger)
        {
        }
    }
}