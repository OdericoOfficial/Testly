using System.Security.Cryptography;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Policies
{
    internal class IncreasePolicy<TCommand> : IIncreasePolicy<TCommand>
        where TCommand : IIncreaseCommand
    {
        public async Task ScheduleAsync(TCommand item, Func<Task> sentTask)
        {
            var historyTask = 0;
            var currentTask = item.Step;
            while (historyTask + currentTask < item.Sample)
            {
                _ = Parallel.ForAsync(0, currentTask, async (index, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await sentTask();
                });
                await Task.Delay(RandomNumberGenerator.GetInt32(item.DelayInclusive, item.DelayExclusive));
                historyTask += currentTask;
            }

            if (item.Sample - historyTask > 0)
            {
                _ = Parallel.ForAsync(0, item.Sample - historyTask, async (index, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await sentTask();
                });
            }
        }
    }
}