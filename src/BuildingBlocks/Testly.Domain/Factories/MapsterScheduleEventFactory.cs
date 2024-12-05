using MapsterMapper;
using Testly.Domain.Events;

namespace Testly.Domain.Factories
{
    internal class MapsterScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse> : IScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        private readonly IMapper _mapper;

        public MapsterScheduleEventFactory(IMapper mapper)
            => _mapper = mapper;

        public TSentEvent CreateSentEvent(TRequest request, (DateTime SendingTime, DateTime SentTime) tuple, Guid aggregateId, int sentIndex)
        {
            var sentEvent = _mapper.Map<TRequest, TSentEvent>(request);
            sentEvent.SendingTime = tuple.SendingTime;
            sentEvent.SentTime = tuple.SentTime;
            sentEvent.AggregateId = aggregateId;
            sentEvent.SentIndex = sentIndex;
            return sentEvent;
        }

        public TReceivedEvent CreateReceivedEvent(TResponse response, TSentEvent sentEvent, DateTime receivedTime, int receivedIndex)
        {
            var receivedEvent = _mapper.Map<TResponse, TReceivedEvent>(response);
            receivedEvent.ReceivedTime = receivedTime;
            receivedEvent.ReceivedIndex = receivedIndex;
            receivedEvent.AggregateId= sentEvent.AggregateId;
            return receivedEvent;
        }
    }
}
