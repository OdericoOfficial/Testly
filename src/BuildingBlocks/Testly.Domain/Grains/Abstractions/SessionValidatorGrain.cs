using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class SessionValidatorGrain<TSentEvent, TReceivedEvent> : SentValidatorGrain<TSentEvent, SessionState<TSentEvent, TReceivedEvent>>, 
        IDomainEventAsyncObserver<TReceivedEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        private readonly IAsyncObserver<TReceivedEvent> _observer;
        
        private StreamSubscriptionHandle<TReceivedEvent>? _subscriptionHandle;

        protected SessionValidatorGrain(IAsyncObserver<TSentEvent> sentObserver, IAsyncObserver<TReceivedEvent> receivedObserver,
            ILogger logger) : base(sentObserver, logger)
            => _observer = receivedObserver;
        
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            
            _subscriptionHandle = await _streamProvider.GetStream<TReceivedEvent>(Constants.DefaultSessionValidatorNamespace, this.GetPrimaryKey())
                .SubscribeAsync(_observer);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await _subscriptionHandle!.UnsubscribeAsync();

            await base.OnDeactivateAsync(reason, cancellationToken);
        }

        public override Task OnNextAsync(TSentEvent item)
        {
            switch (State.Process)
            {
                case SessionProcess.None:
                    State.ApplyNoneToContainSentEvent(item);
                    return Task.CompletedTask;
                case SessionProcess.ContainReceivedEvent:
                    State.ApplyContainReceivedEventToBothContained(item);
                    return OnCompletedAsync();
            }

            return Task.CompletedTask;
        }

        public Task OnNextAsync(TReceivedEvent item)
        {
            switch (State.Process)
            {
                case SessionProcess.None:
                    State.ApplyNoneToContainReceivedEvent(item);
                    return Task.CompletedTask;
                case SessionProcess.ContainSentEvent:
                    State.ApplyContainSentEventToBothContained(item);
                    return OnCompletedAsync();
            }
            return Task.CompletedTask;
        }

        protected override async Task OnCompletedAsync()
        {
            if (_streamProvider != null
                && State.Process == SessionProcess.BothContained
                && ValidateSession(State.SentEvent, State.ReceivedEvent))
            {
                var aggregateStream = _streamProvider.GetStream<AggregateUnitEvent>(Constants.DefaultAggregateNamespace, 
                    State.ReceivedEvent.PublisherId);

                await aggregateStream.OnNextAsync(new AggregateUnitEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.ReceivedEvent.ReceivedTime,
                    PublisherId = this.GetPrimaryKey(),
                    SubscriberId = State.ReceivedEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSession(TSentEvent sentEvent, TReceivedEvent receivedEvent);
    }
}
