namespace Testly.Domain.Grains
{
    public interface IAggregateGrain : IGrainWithGuidKey
    {
        Task StartMeasurementAsync(int sample, int batchSize);
    }
}
