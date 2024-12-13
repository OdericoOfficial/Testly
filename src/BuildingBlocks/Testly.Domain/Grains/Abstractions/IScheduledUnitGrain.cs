using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduledUnitGrain<TCommand> : IScheduledNodeGrain<TCommand>
        where TCommand : IModifyUnitCommand
    {
    }
}
