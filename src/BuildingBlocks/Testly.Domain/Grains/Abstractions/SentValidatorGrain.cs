using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class SentValidatorGrain<TSentEvent, TState> : Grain<TState>, IDomainEventAsyncObserver<TSentEvent>
        where TSentEvent : struct, ISentEvent
        where TState : ISentState<TSentEvent>
    {
        private readonly IAsyncObserver<TSentEvent> _observer;

        protected readonly ILogger _logger;

        private StreamSubscriptionHandle<TSentEvent>? _subscriptionHandle;
        
        protected IStreamProvider? _streamProvider;
        
        protected SentValidatorGrain(IAsyncObserver<TSentEvent> observer, ILogger logger)
        {
            _observer = observer;
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            DelayDeactivation(TimeSpan.FromMinutes(2));
            
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _subscriptionHandle = await _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, this.GetPrimaryKey())
                .SubscribeAsync(_observer);
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await _subscriptionHandle!.UnsubscribeAsync();
            
            await ClearStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, this.GetPrimaryKey(), reason);
        }

        public virtual Task OnNextAsync(TSentEvent item)
        {
            switch (State.Process)
            {
                case SessionProcess.None:
                    State.ApplyNoneToContainSentEvent(item);
                    return OnCompletedAsync();
            }
            return Task.CompletedTask;
        }

        protected virtual async Task OnCompletedAsync()
        {
            if (_streamProvider != null
                && State.Process == SessionProcess.ContainSentEvent)
            {
                var aggregateStream = _streamProvider.GetStream<AggregateUnitEvent>(Constants.DefaultAggregateNamespace,
                    State.SentEvent.PublisherId);

                await aggregateStream.OnNextAsync(new AggregateUnitEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.SentEvent.SentTime,
                    PublisherId = this.GetPrimaryKey(),
                    SubscriberId = State.SentEvent.PublisherId
                });
            }
        }
    }
}
