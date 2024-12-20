﻿using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Observers.Abstractions;
using Testly.Domain.States;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [GrainWithGuidKey]
    [StreamProvider]
    public abstract partial class SimplexValidatorGrain<TSentEvent> : Grain<SentState<TSentEvent>>, 
        IEventObserver<TSentEvent>,
        IRougamo<LoggingException>
        where TSentEvent : SentEvent
    {
        protected readonly ILogger _logger;

        protected SimplexValidatorGrain(ILogger logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                GetType().Name, GrainId, reason);
            
            await ClearStateAsync();
        }

        public async Task OnNextAsync(TSentEvent item)
        {
            switch (State.ContainState)
            {
                case SentContainState.ContainSentEvent:
                    State.ApplyNoneToContainSentEvent(item);
                    await OnCompletedAsync();
                    break;
            }
        }

        private async Task OnCompletedAsync()
        {
            if (State.ContainState == SentContainState.ContainSentEvent
                && State.SentEvent is not null
                && ValidateSent(State.SentEvent))
            {
                var measurementUnitStream = StreamProvider.GetStream<MeasurementCompletedEvent>(State.SentEvent.PublisherId);

                await measurementUnitStream.OnNextAsync(new MeasurementCompletedEvent
                {
                    StartTime = State.SentEvent.SendingTime,
                    EndTime = State.SentEvent.SentTime,
                    PublisherId = GrainId,
                    SubscriberId = State.SentEvent.PublisherId
                });
            }
        }

        protected abstract bool ValidateSent(TSentEvent sentEvent);
    }
}
