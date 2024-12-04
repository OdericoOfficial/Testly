using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.States;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(nameof(AggregateEvent))]
    internal class AggregateGrain : Grain<AggregateState>, IAggregateGrain, IAsyncObserver<AggregateEvent>, IRemindable
    {
        private readonly ILogger<AggregateGrain> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<AggregateEvent>? _aggregateStream;
        private StreamSubscriptionHandle<AggregateEvent>? _subscriptionHandle;

        public AggregateGrain(ILogger<AggregateGrain> logger)
        {
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _aggregateStream = _streamProvider.GetStream<AggregateEvent>(nameof(AggregateEvent), this.GetPrimaryKey());
            _subscriptionHandle = await _aggregateStream.SubscribeAsync(this);

        }

        public Task OnNextAsync(AggregateEvent item, StreamSequenceToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task OnErrorAsync(Exception ex)
        {
            throw new NotImplementedException();
        }

        public Task OnCompletedAsync()
        {
            throw new NotImplementedException();
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
