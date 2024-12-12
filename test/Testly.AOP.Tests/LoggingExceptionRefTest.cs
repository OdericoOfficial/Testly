using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests
{
    public class LoggingExceptionRefTest
    {
        private readonly IEnumerable<IThrowExceptionRefMock> _mocks;

        public LoggingExceptionRefTest(IEnumerable<IThrowExceptionRefMock> mocks)
            => _mocks = mocks;

        [Fact]
        public void TestNotThrow()
        {
            foreach (var item in _mocks)
                item.NotThrow();
        }

        [Fact]
        public async Task TestNotThrowAsync()
        {
            foreach (var item in _mocks)
                await item.NotThrowAsync();
        }

        [Fact]
        public void TestThrowException()
        {
            foreach (var item in _mocks)
                item.ThrowException();
        }

        [Fact]
        public async Task TestThrowExceptionAsync()
        {
            foreach (var item in _mocks)
                await item.ThrowExceptionAsync();
        }
    }
}
