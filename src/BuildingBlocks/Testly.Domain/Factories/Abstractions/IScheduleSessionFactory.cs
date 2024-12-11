using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Factories.Abstractions
{
    public interface IScheduleSessionFactory<TRequest, TCommand>
        where TCommand : struct, IModifyUnitCommand
    {
        TRequest Create(TCommand command, Guid unitId);

        Func<TCommand, TRequest, Task<(DateTime SendingTime, DateTime SentTime)>> CreateAsyncInvoker();
    }
}
