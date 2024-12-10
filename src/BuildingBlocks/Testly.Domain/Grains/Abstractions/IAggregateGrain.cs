namespace Testly.Domain.Grains.Abstractions
{
    public interface IAggregateGrain : IGrainWithGuidKey
    {
        Task StartMeasurementAsync(Guid unitId, int sample, int batchSize);
    }
}
