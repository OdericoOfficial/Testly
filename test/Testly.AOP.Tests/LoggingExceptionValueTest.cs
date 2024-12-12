using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests
{
    public class LoggingExceptionValueTest
    {
        private readonly IEnumerable<IThrowExceptionValueMock> _mocks;

        public LoggingExceptionValueTest(IEnumerable<IThrowExceptionValueMock> mocks)
            => _mocks = mocks;

        [Fact]
        public void TestNotThrow()
        {
            foreach (var item in _mocks)
                Assert.Equal(0, item.NotThrow());
        }

        [Fact]
        public async Task TestNotThrowAsync()
        {
            foreach (var item in _mocks)
                Assert.Equal(0, await item.NotThrowAsync());
        }

        [Fact]
        public void TestThrowException()
        {
            foreach (var item in _mocks)
                Assert.Equal(default(int), item.ThrowException());
        }

        [Fact]
        public async Task TestThrowExceptionAsync()
        {
            foreach (var item in _mocks)
                Assert.Equal(default(int), await item.ThrowExceptionAsync());
        }
    }
}
