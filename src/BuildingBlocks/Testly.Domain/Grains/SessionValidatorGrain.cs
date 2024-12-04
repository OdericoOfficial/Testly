using Orleans;
using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States;

namespace Testly.Domain.Grains
{
    public abstract class SessionValidatorGrain<TSentEvent, TReceivedEvent> : Grain<SessionState<TSentEvent, TReceivedEvent>>, ISessionValidatorGrain<TSentEvent, TReceivedEvent>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        private IStreamProvider? _streamProvider;
        private IAsyncStream<AggregateEvent>? _aggregateStream;

        public async Task CacheReceivedEventAsync(TReceivedEvent receivedEvent)
        {
            State.ReceivedEvent = receivedEvent;
            if (State.SentEvent is not null)
                await PublishAsync();
            else
                await WriteStateAsync();
        }

        public async Task CacheSentEventAsync(TSentEvent sentEvent)
        {
            State.SentEvent = sentEvent;
            if (State.ReceivedEvent is not null)
                await PublishAsync();
            else
                await WriteStateAsync();
        }

        private async Task PublishAsync()
        {
            if (ValidateSession(State.SentEvent!, State.ReceivedEvent!))
            {
                var aggregateStream = GetAggregateStream();
                await aggregateStream.OnNextAsync(new AggregateEvent
                {
                    SentTime = State.SentEvent!.SentTime,
                    ReceivedTime = State.ReceivedEvent!.ReceivedTime,
                    SentIndex = State.SentEvent.SentIndex,
                    ReceivedIndex = State.ReceivedEvent.ReceivedIndex
                });
            }
        }

        private IAsyncStream<AggregateEvent> GetAggregateStream()
        {
            if (_streamProvider is null)
                _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            if (_aggregateStream is null)
                _aggregateStream = _streamProvider.GetStream<AggregateEvent>(nameof(AggregateEvent), State.ReceivedEvent!.AggregateId);
            return _aggregateStream;
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
