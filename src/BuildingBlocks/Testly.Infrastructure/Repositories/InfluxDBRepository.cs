using InfluxDB.Client;
using InfluxDB.Client.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Testly.Domain.Repositories;

namespace Testly.Infrastructure.Repositories
{
    internal class InfluxDBRepository<TEntity> : IWritableRepository<TEntity>
    {
        private readonly IServiceProvider _provider;
        private readonly InfluxDBClientOptions _options;

        public InfluxDBRepository(IServiceProvider provider, IOptions<InfluxDBClientOptions> options)
        {
            _provider = provider;
            _options = options.Value;
        }

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var writeApi = _provider.GetRequiredService<WriteApiAsync>();
            return writeApi.WriteMeasurementAsync(entity, cancellationToken: cancellationToken);
        }

        public IQueryable<TEntity> AsQueryable()
            => InfluxDBQueryable<TEntity>.Queryable(_options.Bucket, _options.Org, _provider.GetRequiredService<QueryApi>());
    }
}
