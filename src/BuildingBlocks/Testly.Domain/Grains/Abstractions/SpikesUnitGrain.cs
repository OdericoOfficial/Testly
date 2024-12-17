using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class SpikesUnitGrain<TSentEvent, TRequest, TModifyCommand> : ScheduledUnitGrain<TSentEvent, TRequest, TModifyCommand>
        where TSentEvent : SentEvent
        where TModifyCommand : ModifySpikeUnitCommand
    {
        protected SpikesUnitGrain(ILogger logger,
            IScheduleSessionFactory<TRequest, TModifyCommand> sessionFactory,
            ISchduleSentEventFactory<TSentEvent, TRequest> sentFactory) : base(logger, sessionFactory, sentFactory)
        {
        }

        protected override async Task InternalScheduleAsync(Func<Task> sessionTask)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing
                && State.Command is not null)
            {
                await Parallel.ForAsync(0, State.Command.Sample, async (index, cancellationToken) =>
                {
                    await Task.Delay(RandomNumberGenerator.GetInt32(State.Command.DelayInclusive, State.Command.DelayExclusive));
                    await sessionTask();
                });
            }
        }
    }
}