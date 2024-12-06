using Testly.Domain.Commands;

namespace Testly.Domain.Factories.Abstractions
{
    public interface IScheduleSessionFactory<TRequest, TCommand>
        where TCommand : ScheduleCommand
    {
        TRequest Create(TCommand command, Guid aggregateId);

        Func<TCommand, TRequest, Task<(DateTime SendingTime, DateTime SentTime)>> CreateAsyncInvoker();
    }
}
