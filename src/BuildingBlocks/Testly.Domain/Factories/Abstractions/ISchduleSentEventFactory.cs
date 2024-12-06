using Testly.Domain.Events;

namespace Testly.Domain.Factories.Abstractions
{
    public interface ISchduleSentEventFactory<TSentEvent, TRequest>
        where TSentEvent : SentEvent
    {
        ValueTask<TSentEvent> CreateAsync(TRequest request, (DateTime SendingTime, DateTime SentTime) tuple, Guid aggregateId);
    }
}
