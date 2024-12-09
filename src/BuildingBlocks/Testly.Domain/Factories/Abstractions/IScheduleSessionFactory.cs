using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Factories.Abstractions
{
    public interface IScheduleSessionFactory<TRequest, TCommand>
        where TCommand : struct, IScheduleUnitCommand
    {
        TRequest Create(TCommand command, Guid aggregateId);

        Func<TCommand, TRequest, Task<(DateTime SendingTime, DateTime SentTime)>> CreateAsyncInvoker();
    }
}
