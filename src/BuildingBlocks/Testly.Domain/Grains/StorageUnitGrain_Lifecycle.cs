using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.Repositories.Abstractions;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription]
    internal partial class StorageUnitGrain : Grain, 
        IDomainEventAsyncObserver<ScalarEvent>, 
        IDomainEventAsyncObserver<SummaryEvent>,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
    {
        private readonly ILogger _logger;
        private readonly IAsyncObserver<ScalarEvent> _scalarObserver;
        private readonly IAsyncObserver<SummaryEvent> _summaryObserver;
        private readonly IWriteOnlyRepository<ScalarEvent> _scalarRepository;
        private readonly IWriteOnlyRepository<SummaryEvent> _summaryRepository;
        private Guid? _storageId;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<ScalarEvent>? _scalarStream;
        private IAsyncStream<SummaryEvent>? _summaryStream;
        private IAsyncStream<ScheduledNodeCompletedEvent>? _nodeCompletedStream;
        private StreamSubscriptionHandle<ScalarEvent>? _scalarHandle;
        private StreamSubscriptionHandle<SummaryEvent>? _summaryHandle;
        private Guid _unitId;

        private Guid StorageId
            => _storageId ??= this.GetPrimaryKey();

        private IStreamProvider StreamProvider
            => _streamProvider ??= this.GetStreamProvider(nameof(Stream));

        private IAsyncStream<ScalarEvent> ScalarStream
            => _scalarStream ??= StreamProvider.GetStream<ScalarEvent>(StorageId);

        private IAsyncStream<SummaryEvent> SummaryStream
            => _summaryStream ??= StreamProvider.GetStream<SummaryEvent>(StorageId);

        private IAsyncStream<ScheduledNodeCompletedEvent> NodeCompletedStream
            => _nodeCompletedStream ??= StreamProvider.GetStream<ScheduledNodeCompletedEvent>(_unitId);

        public StorageUnitGrain(ILogger<StorageUnitGrain> logger, 
            IAsyncObserver<ScalarEvent> scalarObserver, 
            IAsyncObserver<SummaryEvent> summaryObserver,
            IWriteOnlyRepository<ScalarEvent> scalarRepository, 
            IWriteOnlyRepository<SummaryEvent> summaryRepository)
        {
            _logger = logger;
            _scalarObserver = scalarObserver;
            _summaryObserver = summaryObserver;
            _scalarRepository = scalarRepository;
            _summaryRepository = summaryRepository;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _scalarHandle = await ScalarStream.SubscribeAsync(_scalarObserver);
            _summaryHandle = await SummaryStream.SubscribeAsync(_summaryObserver);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (_unitId != default)
                await NodeCompletedStream.OnNextAsync(new ScheduledNodeCompletedEvent
                {
                    PublisherId = this.GetPrimaryKey(),
                    SubscriberId = _unitId
                });

            await UnsubscribeSetNullAsync(ref _scalarHandle);
            await UnsubscribeSetNullAsync(ref _summaryHandle);

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(StorageUnitGrain), this.GetPrimaryKey(), reason);
        }
    }
}
