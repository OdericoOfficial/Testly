using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Factories.Abstractions
{
    public interface ISentFactory<TRequest, TCommand>
        where TCommand : IUnitCommand
    {
        TRequest Create(TCommand item, Guid unitId);

        Func<TRequest, TCommand, Task<(DateTime SendingTime, DateTime SentTime)>> CreateAsyncInvoker();
    }
}
