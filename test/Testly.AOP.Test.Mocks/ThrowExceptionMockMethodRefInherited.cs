using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionRefMock>, Enumerable]
    internal class ThrowExceptionMockMethodRefInherited : ThrowExceptionMockMethodRefAbstract
    {
        public ThrowExceptionMockMethodRefInherited(ILogger<ThrowExceptionMockMethodRefInherited> logger) : base(logger)
        {
        }
    }
}