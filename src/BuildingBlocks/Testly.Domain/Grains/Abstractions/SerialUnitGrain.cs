using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class SerialUnitGrain<TSentEvent, TRequest, TModifyCommand> : ScheduledUnitGrain<TSentEvent, TRequest, TModifyCommand>
        where TSentEvent : SentEvent
        where TModifyCommand : ModifySerialUnitCommand
    {
        protected SerialUnitGrain(ILogger logger,
            IScheduleSessionFactory<TRequest, TModifyCommand> sessionFactory, 
            ISchduleSentEventFactory<TSentEvent, TRequest> sentFactory) : base(logger, sessionFactory, sentFactory)
        {
        }

        protected override async Task InternalScheduleAsync(Func<Task> sessionTask)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing
                && State.Command is not null)
            {
                var count = State.Command.Sample / State.Command.BatchSize;
                var rest = State.Command.Sample % State.Command.BatchSize;
                for (var i = 0; i < count; i++)
                {
                    await Parallel.ForAsync(0, State.Command.BatchSize, async (index, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(RandomNumberGenerator.GetInt32(State.Command.DelayInclusive, State.Command.DelayExclusive));
                        await sessionTask();
                    });
                }

                if (rest > 0)
                {
                    await Parallel.ForAsync(0, rest, async (index, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(RandomNumberGenerator.GetInt32(State.Command.DelayInclusive, State.Command.DelayExclusive));
                        await sessionTask();
                    });
                }
            }
        }
    }
}
