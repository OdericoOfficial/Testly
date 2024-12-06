using MapsterMapper;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    internal class MapsterScheduleSentEventFactory<TSentEvent, TRequest> : ISchduleSentEventFactory<TSentEvent, TRequest>
        where TSentEvent : SentEvent
    {
        private readonly IMapper _mapper;
        private readonly IGuidFactory _guidFactory;  

        public MapsterScheduleSentEventFactory(IMapper mapper, IGuidFactory guidFactory)
        {
            _mapper = mapper;
            _guidFactory = guidFactory;
        }

        public async ValueTask<TSentEvent> CreateAsync(TRequest request, (DateTime SendingTime, DateTime SentTime) tuple, Guid aggregateId)
        {
            var sentEvent = _mapper.Map<TRequest, TSentEvent>(request);
            sentEvent.SendingTime = tuple.SendingTime;
            sentEvent.SentTime = tuple.SentTime;
            sentEvent.AggregateId = aggregateId;
            sentEvent.ValidatorId = await _guidFactory.NextAsync();
            return sentEvent;
        }
    }
}
