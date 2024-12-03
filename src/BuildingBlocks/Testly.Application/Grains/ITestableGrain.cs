using Orleans;
namespace Testly.Application.Grains
{
    public interface ITestableGrain<TSentDomainEvent, TReceivedDomainEvent> : IGrainWithGuidKey
    {
        Task PublishSentEventAsync<TDto>(TDto dto, CancellationToken cancellationToken = default);

        Task PublishReceivedEventAsync<TDto>(TDto dto, CancellationToken cancellationToken = default);
    }
}
