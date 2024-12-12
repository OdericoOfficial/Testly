namespace Testly.Domain.Repositories.Abstractions
{
    public interface IWriteOnlyRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        Task AddAsync(TEntity entity);
    }
}
