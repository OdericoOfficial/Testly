using Testly.Domain.Commands;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleGroupGrain
    {
        Task ModifyGroupAsync(ModifyGroupCommand command);

        Task AddLayerAsync(ModifyLayerCommand command);

        Task CancelAsync();

        Task ClearAsync();
    }
}
