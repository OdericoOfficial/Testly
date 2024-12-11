namespace Testly.Domain.Grains.Abstractions
{
    public interface IAggregateUnitGrain : IGrainWithGuidKey
    {
        Task ExecuteAsync(int sample, int batchSize);
    }
}
