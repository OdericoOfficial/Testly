using Testly.Domain.Commands.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public class ScheduledUnitState<TCommand> : ScheduledNodeState<TCommand>
        where TCommand : ModifyScheduledUnitCommand
    {
    }
}
