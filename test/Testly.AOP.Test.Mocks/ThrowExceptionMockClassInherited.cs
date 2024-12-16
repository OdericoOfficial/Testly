using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>, Enumerable]
    internal class ThrowExceptionMockClassInherited : ThrowExceptionMockClassAbstract
    {
        public ThrowExceptionMockClassInherited(ILogger<ThrowExceptionMockClassInherited> logger) : base(logger)
        {
        }
    }
}