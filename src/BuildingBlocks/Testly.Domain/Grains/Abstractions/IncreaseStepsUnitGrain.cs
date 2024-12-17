using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class IncreaseStepsUnitGrain<TSentEvent, TRequest, TModifyCommand> : ScheduledUnitGrain<TSentEvent, TRequest, TModifyCommand>
    where TSentEvent : SentEvent
    where TModifyCommand : ModifyIncreaseStepsUnitCommand
    {
        protected IncreaseStepsUnitGrain(ILogger logger,
            IScheduleSessionFactory<TRequest, TModifyCommand> sessionFactory,
            ISchduleSentEventFactory<TSentEvent, TRequest> sentFactory) : base(logger, sessionFactory, sentFactory)
        {
        }

        protected override async Task InternalScheduleAsync(Func<Task> sessionTask)
        {
            if (State.CurrentState == ScheduledNodeCurrentState.Executing
                && State.Command is not null)
            {
                var historyTask = 0;
                var currentTask = State.Command.Step;
                while (historyTask + currentTask < State.Command.Sample)
                {
                    _ = Parallel.ForAsync(0, currentTask, async (index, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await sessionTask();
                    });
                    await Task.Delay(RandomNumberGenerator.GetInt32(State.Command.DelayInclusive, State.Command.DelayExclusive));
                    historyTask += currentTask;
                }

                if (State.Command.Sample - historyTask > 0)
                {
                    _ = Parallel.ForAsync(0, State.Command.Sample - historyTask, async (index, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await sessionTask();
                    });
                }
            }
        }
    }

}
