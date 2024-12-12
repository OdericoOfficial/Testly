using Microsoft.Extensions.Logging;
using Testly.AOP.Tests.Mocks.Abstractions;
using Testly.DependencyInjection.Attributes;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionMock>]
    internal class ThrowExceptionMockMethodInherited : ThrowExceptionMockMethodAbstract
    {
        public ThrowExceptionMockMethodInherited(ILogger<ThrowExceptionMockMethodInherited> logger) : base(logger)
        {
        }
    }
}
