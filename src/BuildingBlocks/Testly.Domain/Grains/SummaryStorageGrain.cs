using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.Repositories.Abstractions;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(Constants.DefaultScalarNamespace)]
    [ImplicitStreamSubscription(Constants.DefaultSummaryNamespcace)]
    internal class SummaryStorageGrain : ScopedGrain, IDomainEventAsyncObserver<ScalarEvent>, IDomainEventAsyncObserver<SummaryEvent>,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
    {
        private readonly ILogger _logger;
        private readonly IAsyncObserver<ScalarEvent> _scalarObserver;
        private readonly IAsyncObserver<SummaryEvent> _summaryObserver;
        private IWriteOnlyRepository<ScalarEvent>? _scalarRepository;
        private IWriteOnlyRepository<SummaryEvent>? _summaryRepository;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<ScalarEvent>? _scalarStream;
        private IAsyncStream<SummaryEvent>? _summaryStream;
        private IAsyncStream<StorageCompletedEvent>? _storageCompletedStream;
        private StreamSubscriptionHandle<ScalarEvent>? _scalarHandle;
        private StreamSubscriptionHandle<SummaryEvent>? _summaryHandle;
        private Guid _unitId;

        public SummaryStorageGrain(ILogger<SummaryStorageGrain> logger, IAsyncObserver<ScalarEvent> scalarObserver, IAsyncObserver<SummaryEvent> summaryObserver)
        {
            _logger = logger;
            _scalarObserver = scalarObserver;
            _summaryObserver = summaryObserver;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            var storageId = this.GetPrimaryKey();

            _scalarRepository = ScopedProvider.GetRequiredService<IWriteOnlyRepository<ScalarEvent>>();
            _summaryRepository = ScopedProvider.GetRequiredService<IWriteOnlyRepository<SummaryEvent>>();
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _scalarStream = _streamProvider.GetStream<ScalarEvent>(Constants.DefaultScalarNamespace, storageId);
            _summaryStream = _streamProvider.GetStream<SummaryEvent>(Constants.DefaultScalarNamespace, storageId);
            _scalarHandle = await _scalarStream.SubscribeAsync(_scalarObserver);
            _summaryHandle = await _summaryStream.SubscribeAsync(_summaryObserver); 
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(reason, cancellationToken);
            await _scalarHandle!.UnsubscribeAsync();
            await _summaryHandle!.UnsubscribeAsync();

            if (_storageCompletedStream is not null && _unitId != default)
                await _storageCompletedStream.OnNextAsync(new StorageCompletedEvent
                {
                    PublisherId = this.GetPrimaryKey(),
                    SubscriberId = _unitId,
                    CompletedTime = DateTime.UtcNow
                });

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(SummaryStorageGrain), this.GetPrimaryKey(), reason);
        }

        public async Task OnNextAsync(ScalarEvent item)
        {
            await _scalarRepository!.AddAsync(item);
            _unitId = item.PublisherId;
        }

        public async Task OnNextAsync(SummaryEvent item)
        {
            await _summaryRepository!.AddAsync(item);
            _unitId = item.PublisherId;
            _storageCompletedStream ??= _streamProvider.GetStream<StorageCompletedEvent>(Constants.DefaultStorageCompletedNamespace, this.GetPrimaryKey());
        }
    }
}
