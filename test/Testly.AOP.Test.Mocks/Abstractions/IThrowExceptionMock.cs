namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public interface IThrowExceptionMock
    {
        void NotThrow();

        Task NotThrowAsync();

        void ThrowException();

        Task ThrowExceptionAsync();
    }
}
