namespace Testly.Domain.Repositories.Abstractions
{
    public interface IReadOnlyRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        IQueryable<TEntity> AsQueryable();
    }
}
