using Testly.Domain.Commands;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleGroupGrain : IGrainWithGuidKey
    {
        Task ModifyGroupAsync(ModifyGroupCommand command);

        Task<Guid> AddLayerAsync(AddLayerCommand command);

    }
}
