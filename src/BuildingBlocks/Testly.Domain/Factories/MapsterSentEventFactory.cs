using MapsterMapper;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    internal class MapsterSentEventFactory<TSentEvent, TRequest> : ISentEventFactory<TSentEvent, TRequest>
        where TSentEvent : SentEvent
    {
        private readonly IMapper _mapper;
        private readonly IGuidFactory _guidFactory;

        public MapsterSentEventFactory(IMapper mapper, IGuidFactory guidFactory)
        {
            _mapper = mapper;
            _guidFactory = guidFactory;
        }

        public async ValueTask<TSentEvent> CreateAsync(TRequest request, (DateTime SendingTime, DateTime SentTime) tuple, Guid unitId)
            => _mapper.Map<TRequest, TSentEvent>(request) with
            {
                SendingTime = tuple.SendingTime,
                SentTime = tuple.SentTime,
                PublisherId = unitId,
                SubscriberId = await _guidFactory.NextAsync()
            };
    }
}
