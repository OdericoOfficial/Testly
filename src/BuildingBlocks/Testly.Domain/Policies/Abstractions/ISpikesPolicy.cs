using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Policies.Abstractions
{
    public interface ISpikesPolicy<TCommand> : ISentPolicy<TCommand>
        where TCommand : ISpikeCommand
    {
    }
}
