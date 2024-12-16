using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using static Testly.Domain.Grains.NullSetter;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent> : Grain<SessionState<TSentEvent, TReceivedEvent>>,
        IDomainEventAsyncObserver<TReceivedEvent>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        protected readonly ILogger _logger;
        private readonly IAsyncObserver<TSentEvent> _sentObserver;
        private readonly IAsyncObserver<TReceivedEvent> _receivedObserver;
        private Guid? _validatorId;
        private IStreamProvider? _streamProvider;
        private IAsyncStream<TSentEvent>? _sentStream;
        private IAsyncStream<TReceivedEvent>? _receivedStream;
        private StreamSubscriptionHandle<TSentEvent>? _sentHandle;
        private StreamSubscriptionHandle<TReceivedEvent>? _receivedHandle;

        private Guid ValidatorId
            => _validatorId ??= this.GetPrimaryKey();

        private IStreamProvider StreamProvider
            => _streamProvider ??= this.GetStreamProvider(nameof(Stream));

        private IAsyncStream<TSentEvent> SentStream
            => _sentStream ??= StreamProvider.GetStream<TSentEvent>(ValidatorId);

        private IAsyncStream<TReceivedEvent> ReceivedStream
            => _receivedStream ??= StreamProvider.GetStream<TReceivedEvent>(ValidatorId);

        protected SessionValidatorGrain(IAsyncObserver<TSentEvent> sentObserver, IAsyncObserver<TReceivedEvent> receivedObserver, ILogger logger)
        {
            _sentObserver = sentObserver;
            _receivedObserver = receivedObserver;
            _logger = logger;   
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _sentHandle = await SentStream.SubscribeAsync(_sentObserver);
            _receivedHandle = await ReceivedStream.SubscribeAsync(_receivedObserver);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await UnsubscribeSetNullAsync(ref _sentHandle);
            await UnsubscribeSetNullAsync(ref _receivedHandle);
            await ClearStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, ValidatorId, reason);
        }
    }
}