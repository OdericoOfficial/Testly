using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Factories.Abstractions
{
    public interface ISentFactory<TRequest>
    {
        TRequest Create(Guid unitId);

        Func<TRequest, Task<(DateTime SendingTime, DateTime SentTime)>> CreateAsyncInvoker();
    }
}
