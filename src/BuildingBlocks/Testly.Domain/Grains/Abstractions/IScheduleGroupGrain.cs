using Testly.Domain.Commands;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleGroupGrain : IGrainWithGuidKey
    {
        Task ModifyGroupAsync(ModifyGroupCommand command);

        Task<Guid> AddLayerAsync(ModifyLayerCommand command);

        Task CancelAsync();
    }
}
