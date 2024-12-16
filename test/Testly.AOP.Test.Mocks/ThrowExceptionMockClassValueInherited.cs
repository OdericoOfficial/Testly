using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>, Enumerable]
    internal class ThrowExceptionMockClassValueInherited : ThrowExceptionMockClassValueAbstract
    {
        public ThrowExceptionMockClassValueInherited(ILogger<ThrowExceptionMockClassValueInherited> logger) : base(logger)
        {
        }
    }
}