namespace Testly.Domain.Repositories
{
    public interface IReadOnlyRepository<TEntity>
    {
        IQueryable<TEntity> AsQueryable();
    }
}
