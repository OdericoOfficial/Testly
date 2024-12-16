using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>, Enumerable]
    internal class ThrowExceptionMockMethodValueInherited : ThrowExceptionMockMethodValueAbstract
    {
        public ThrowExceptionMockMethodValueInherited(ILogger<ThrowExceptionMockMethodValueInherited> logger) : base(logger)
        {
        }
    }
}