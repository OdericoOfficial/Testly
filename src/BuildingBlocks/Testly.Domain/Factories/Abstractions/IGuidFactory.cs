namespace Testly.Domain.Factories.Abstractions
{
    public interface IGuidFactory
    {
        ValueTask<Guid> NextAsync();
    }
}
