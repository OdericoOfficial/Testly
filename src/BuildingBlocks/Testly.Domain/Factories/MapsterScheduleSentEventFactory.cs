using System.Runtime.CompilerServices;
using MapsterMapper;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    internal class MapsterScheduleSentEventFactory<TSentEvent, TRequest> : ISchduleSentEventFactory<TSentEvent, TRequest>
        where TSentEvent : struct, ISentEvent
    {
        private readonly IMapper _mapper;
        private readonly IGuidFactory _guidFactory;  

        public MapsterScheduleSentEventFactory(IMapper mapper, IGuidFactory guidFactory)
        {
            _mapper = mapper;
            _guidFactory = guidFactory;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TSentEvent> CreateAsync(TRequest request, (DateTime SendingTime, DateTime SentTime) tuple, Guid aggregateId)
            => _mapper.Map<TRequest, TSentEvent>(request) with
            {

                SentTime = tuple.SentTime,
                AggregateId = aggregateId,
                ValidatorId = await _guidFactory.NextAsync()
            };
    }
}
