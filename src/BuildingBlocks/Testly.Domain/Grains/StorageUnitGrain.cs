using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using TorchSharp.Modules;

namespace Testly.Domain.Grains
{
    [GrainWithGuidKey]
    [StreamProvider]
    [ImplicitSubscribeAsyncStream<ScalarResultEvent>]
    [ImplicitSubscribeAsyncStream<SummaryResultEvent>]
    [ImplicitStreamSubscription]
    internal sealed partial class StorageUnitGrain : Grain, 
        IDomainEventAsyncObserver<ScalarResultEvent>, 
        IDomainEventAsyncObserver<SummaryResultEvent>,
        IRougamo<LoggingException>
    {
        private readonly ILogger _logger;
        private readonly SummaryWriter _writer;
        private Guid _unitId;

        public StorageUnitGrain(ILogger<StorageUnitGrain> logger, SummaryWriter writer)
        {
            _logger = logger;
            _writer = writer;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
            => await SubscribeAllAsync();

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (_unitId != default)
                await StreamProvider.GetStream<ScheduledNodeCompletedEvent>(_unitId).OnNextAsync(new ScheduledNodeCompletedEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = _unitId
                });

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(StorageUnitGrain), this.GetPrimaryKey(), reason);
        }

        public Task OnNextAsync(ScalarResultEvent item)
        {

            _unitId = item.PublisherId;
            return Task.CompletedTask;
        }

        public Task OnNextAsync(SummaryResultEvent item)
        {
        
            _unitId = item.PublisherId;
            return Task.CompletedTask;
        }
    }
}
