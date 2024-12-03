using Mapster;
using MapsterMapper;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace Testly.Application.Grains
{
    public abstract class TestableGrain<TSentDomainEvent, TReceivedDomainEvent> : Grain, ITestableGrain<TSentDomainEvent, TReceivedDomainEvent>, IAsyncDisposable
    {
        private readonly IAsyncObserver<TSentDomainEvent> _sentEventObserver;
        private readonly IAsyncObserver<TReceivedDomainEvent> _receivedEventObserver;
        private readonly IMapper _mapper;

        private IAsyncStream<TSentDomainEvent>? _sentEventStream;
        private IAsyncStream<TReceivedDomainEvent>? _receivedEventStream;
        private StreamSubscriptionHandle<TSentDomainEvent>? _sentEventHandle;
        private StreamSubscriptionHandle<TReceivedDomainEvent>? _receivedEventHandle;

        public TestableGrain(IAsyncObserver<TSentDomainEvent> sentEventObserver, IAsyncObserver<TReceivedDomainEvent> receviedEventObserver, IMapper mapper)
        {
            _sentEventObserver = sentEventObserver;
            _receivedEventObserver = receviedEventObserver;
            _mapper = mapper;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider(nameof(Stream));
            _sentEventStream = streamProvider.GetStream<TSentDomainEvent>(StreamId.Create(typeof(TSentDomainEvent).Name, this.GetPrimaryKey()));
            _receivedEventStream = streamProvider.GetStream<TReceivedDomainEvent>(StreamId.Create(typeof(TReceivedDomainEvent).Name, this.GetPrimaryKey()));
            _sentEventHandle = await _sentEventStream.SubscribeAsync(_sentEventObserver);
            _receivedEventHandle = await _receivedEventStream.SubscribeAsync(_receivedEventObserver);
        }

        public async Task PublishSentEventAsync<TDto>(TDto dto, CancellationToken cancellationToken = default)
        {
            if (_sentEventStream is not null)
            {
                var domainEvent = _mapper.Map<TDto, TSentDomainEvent>(dto);
                AfterSentEventMapping(domainEvent);
                await _sentEventStream.OnNextAsync(domainEvent);
            }
        }

        protected virtual void AfterSentEventMapping(TSentDomainEvent domainEvent)
        {
        }

        public async Task PublishReceivedEventAsync<TDto>(TDto dto, CancellationToken cancellationToken = default)
        {
            if (_receivedEventStream is not null)
            {
                var domainEvent = _mapper.Map<TDto, TReceivedDomainEvent>(dto);
                AfterReceivedEventMapping(domainEvent);
                await _receivedEventStream.OnNextAsync(domainEvent);
            }
        }

        protected virtual void AfterReceivedEventMapping(TReceivedDomainEvent domainEvent)
        {
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (_sentEventHandle is not null)
                await _sentEventHandle.UnsubscribeAsync();
            if (_receivedEventHandle is not null)
                await _receivedEventHandle.UnsubscribeAsync();
        }
    }
}
