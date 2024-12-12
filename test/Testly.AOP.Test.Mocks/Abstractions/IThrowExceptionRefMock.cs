namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public interface IThrowExceptionRefMock
    {
        object NotThrow();

        Task<object> NotThrowAsync();

        object ThrowException();

        Task<object> ThrowExceptionAsync();
    }
}
