using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>, Enumerable]
    internal class ThrowExceptionMockMethodChildValueInherited : ThrowExceptionMockChildValueAbstract
    {
        public ThrowExceptionMockMethodChildValueInherited(ILogger<ThrowExceptionMockMethodChildValueInherited> logger) : base(logger)
        {
        }


        [Rougamo<LoggingExceptionValue<int>>]

        

        public override int NotThrow()
            => 0;


        [Rougamo<LoggingExceptionValue<int>>]

        

        public override async Task<int> NotThrowAsync()
        {
            await Task.Delay(100);
            return 0;
        }


        [Rougamo<LoggingExceptionValue<int>>]

        

        public override int ThrowException()
            => throw new NotImplementedException();


        [Rougamo<LoggingExceptionValue<int>>]

        

        public override async Task<int> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}