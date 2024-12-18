using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Policies.Abstractions
{
    public interface ISerialPolicy<TCommand> : ISentPolicy<TCommand>
        where TCommand : ISerialCommand
    {
    }
}
