using Orleans;

namespace Testly.Core
{
    public interface ITestGroupGrain<TStartDomainEvent, TEndDomainEvent> : IGrainWithGuidKey
    {
        Task SendTestsAsync(CancellationToken cancellationToken = default);
    }
}
