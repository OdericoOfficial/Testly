using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Policies.Abstractions
{
    public interface IIncreasePolicy<TCommand> : ISentPolicy<TCommand>
        where TCommand : IIncreaseCommand
    {
    }
}
