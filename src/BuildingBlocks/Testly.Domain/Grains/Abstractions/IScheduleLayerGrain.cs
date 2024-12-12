using Testly.Domain.Commands;
using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleLayerGrain
    {
        Task ModifyLayerAsync(ModifyLayerCommand command);

        Task AddUnitAsync<TCommand>(TCommand command) where TCommand : struct, IModifyUnitCommand;


    }
}
