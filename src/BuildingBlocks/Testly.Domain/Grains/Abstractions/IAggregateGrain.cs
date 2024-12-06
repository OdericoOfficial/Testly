namespace Testly.Domain.Grains.Abstractions
{
    public interface IAggregateGrain : IGrainWithGuidKey
    {
        Task StartMeasurementAsync(int sample, int batchSize);
    }
}
