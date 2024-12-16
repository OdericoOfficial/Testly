using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SentValidatorGrain<TSentEvent> : Grain<SentState<TSentEvent>>, 
        IDomainEventAsyncObserver<TSentEvent>,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
        where TSentEvent : SentEvent
    {
        protected readonly ILogger _logger;
        private readonly IAsyncObserver<TSentEvent> _observer;
        private Guid? _validatorId;
        private IStreamProvider? _streamProvider;
        [AsyncStream]
        private IAsyncStream<TSentEvent>? _sentStream;
        private StreamSubscriptionHandle<TSentEvent>? _subscriptionHandle;

        private Guid ValidatorId
            => _validatorId ??= this.GetPrimaryKey();

        private IStreamProvider StreamProvider
            => _streamProvider ??= this.GetStreamProvider(nameof(Stream));

        private IAsyncStream<TSentEvent> SentStream
            => _sentStream ??= StreamProvider.GetStream<TSentEvent>(ValidatorId);

        protected SentValidatorGrain(IAsyncObserver<TSentEvent> observer, ILogger logger)
        {
            _observer = observer;
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
            => _subscriptionHandle = await SentStream.SubscribeAsync(_observer);

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await UnsubscribeSetNullAsync(ref _subscriptionHandle);
            await ClearStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, ValidatorId, reason);
        }
    }
}
