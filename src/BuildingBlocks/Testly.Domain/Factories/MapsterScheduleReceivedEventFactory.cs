using MapsterMapper;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    internal class MapsterScheduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse> : ISchduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        private readonly IMapper _mapper;

        public MapsterScheduleReceivedEventFactory(IMapper mapper)
            => _mapper = mapper;

        public TReceivedEvent Create(TResponse response, TSentEvent sentEvent, DateTime receivedTime, int receivedIndex)
        {
            var receivedEvent = _mapper.Map<TResponse, TReceivedEvent>(response);
            receivedEvent.ReceivedTime = receivedTime;
            receivedEvent.ReceivedIndex = receivedIndex;
            receivedEvent.AggregateId = sentEvent.AggregateId;
            receivedEvent.ValidatorId = sentEvent.ValidatorId;
            return receivedEvent;
        }
    }
}
