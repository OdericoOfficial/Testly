using System.Runtime.CompilerServices;
using MapsterMapper;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    internal class MapsterScheduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse> : ISchduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        private readonly IMapper _mapper;

        public MapsterScheduleReceivedEventFactory(IMapper mapper)
            => _mapper = mapper;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TReceivedEvent Create(TResponse response, TSentEvent sentEvent, DateTime receivedTime, int receivedIndex)
            => _mapper.Map<TResponse, TReceivedEvent>(response) with
            {
                ReceivedTime = receivedTime,
                ReceivedIndex = receivedIndex,
                AggregateId = sentEvent.AggregateId,
                ValidatorId = sentEvent.ValidatorId
            };
    }
}
