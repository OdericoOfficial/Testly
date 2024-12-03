namespace Testly.Domain.Repositories
{
    public interface IWritableRepository<TEntity> : IReadOnlyRepository<TEntity>
    {
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
