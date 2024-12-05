using Testly.Domain.Events;

namespace Testly.Domain.Factories
{
    public interface IScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        TSentEvent CreateSentEvent(TRequest request, (DateTime SendingTime, DateTime SentTime) tuple,
            Guid aggregateId, int sentIndex);

        TReceivedEvent CreateReceivedEvent(TResponse response, TSentEvent sentEvent, DateTime receivedTime, int receivedIndex);
    }
}
