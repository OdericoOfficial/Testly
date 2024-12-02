using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace Testly.Core
{
    public abstract class TestGroupGrain<TSentDomainEvent, TReceivedDomainEvent> : Grain, ITestGroupGrain<TSentDomainEvent, TReceivedDomainEvent>
    {
        private readonly IAsyncObserver<TSentDomainEvent> _sentEventObserver;
        private readonly IAsyncObserver<TReceivedDomainEvent> _receviedEventObserver;

        public TestGroupGrain(IAsyncObserver<TSentDomainEvent> sentEventObserver, IAsyncObserver<TReceivedDomainEvent> receviedEventObserver)
        {
            _sentEventObserver = sentEventObserver;
            _receviedEventObserver = receviedEventObserver;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("StreamProvider");
            var grainId = this.GetPrimaryKey();
            var sendEventStreamId = StreamId.Create(typeof(TSentDomainEvent).Name, grainId);
            var receviedEventStreamId = StreamId.Create(typeof(TReceivedDomainEvent).Name, grainId);

            var sendEventStream = streamProvider.GetStream<TSentDomainEvent>(sendEventStreamId);
            var receviedEventStream = streamProvider.GetStream<TReceivedDomainEvent>(receviedEventStreamId);

            await sendEventStream.SubscribeAsync(_sentEventObserver);
            await receviedEventStream.SubscribeAsync(_receviedEventObserver);
        }

        public abstract Task SendTestsAsync(CancellationToken cancellationToken = default);
    }
}
