using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SentValidatorGrain<TSentEvent, TState> : Grain<TState>, 
        IDomainEventAsyncObserver<TSentEvent>,
#if !ROUGAMO_VERSION_5_0_0_OR_GREATER
        IRougamo<LoggingException>
#else
        IRougamo<LoggingExceptionAttribute>
#endif
        where TSentEvent : struct, ISentEvent
        where TState : ISentState<TSentEvent>
    {
        protected readonly ILogger _logger;

        private readonly IAsyncObserver<TSentEvent> _observer;
        private StreamSubscriptionHandle<TSentEvent>? _subscriptionHandle;
        protected IStreamProvider? _streamProvider;

        protected SentValidatorGrain(IAsyncObserver<TSentEvent> observer, ILogger logger)
        {
            _observer = observer;
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _subscriptionHandle = await _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, this.GetPrimaryKey())
                .SubscribeAsync(_observer);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await _subscriptionHandle!.UnsubscribeAsync();

            await ClearStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }
    }
}
