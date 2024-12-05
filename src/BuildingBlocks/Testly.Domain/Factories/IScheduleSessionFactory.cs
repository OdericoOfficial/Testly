using Testly.Domain.Commands;

namespace Testly.Domain.Factories
{
    public interface IScheduleSessionFactory<TRequest, TCommand>
        where TCommand : ScheduleCommand
    {
        TRequest Create(TCommand command, Guid aggregateId, int sentIndex);

        Func<TCommand, TRequest, Task<(DateTime SendingTime, DateTime SentTime)>> GetAsyncInvoker();
    }
}
