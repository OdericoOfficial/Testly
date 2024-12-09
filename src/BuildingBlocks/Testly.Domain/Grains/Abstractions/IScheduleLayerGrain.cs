using Testly.Domain.Commands;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleLayerGrain : IGrainWithGuidKey
    {
        Task AddLayerAsync(AddLayerCommand command);

        Task DeleteLayerAsync();
    }
}
