namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public interface IThrowExceptionValueMock
    {
        int NotThrow();

        Task<int> NotThrowAsync();

        int ThrowException();

        Task<int> ThrowExceptionAsync();
    }
}
