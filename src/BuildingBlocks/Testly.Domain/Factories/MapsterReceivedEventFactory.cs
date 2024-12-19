using System.Runtime.CompilerServices;
using MapsterMapper;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    internal sealed class MapsterReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse> : IReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        private readonly IMapper _mapper;

        public MapsterReceivedEventFactory(IMapper mapper)
            => _mapper = mapper;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TReceivedEvent Create(TResponse response, TSentEvent sentEvent, DateTime receivedTime)
            => _mapper.Map<TResponse, TReceivedEvent>(response) with
            {
                ReceivedTime = receivedTime,
                PublisherId = sentEvent.PublisherId,
                SubscriberId = sentEvent.SubscriberId,
            };
    }
}
