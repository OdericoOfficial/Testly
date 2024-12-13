using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.States.Abstractions
{
    public interface IScheduledNodeState<TCommand>
        where TCommand : IModifyNodeCommand
    {
        DateTime CompletedTime { get; }

        TCommand? Command { get; }

        ScheduledNodeState CurrentState { get; }

        void ApplyModify(TCommand command);

        void ApplyExecute();

        void ApplyCompleted();

        void ApplyCancelled();
    }
}
