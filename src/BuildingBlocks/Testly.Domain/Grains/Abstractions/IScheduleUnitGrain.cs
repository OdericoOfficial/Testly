using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleUnitGrain<TCommand> : IGrainWithGuidKey
        where TCommand : IModifyUnitCommand
    {
        Task ModifyUnitAsync(TCommand command);

        Task ClearUnitAsync();

        Task ExecuteAsync();
    }
}
