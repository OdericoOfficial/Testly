using System.Security.Cryptography;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Policies
{
    internal class SpikesPolicy<TCommand> : ISpikesPolicy<TCommand>
        where TCommand : ISpikeCommand
    {
        public async Task ScheduleAsync(TCommand item, Func<Task> sentTask)
        {
            await Parallel.ForAsync(0, item.Sample, async (index, cancellationToken) =>
            {
                await Task.Delay(RandomNumberGenerator.GetInt32(item.DelayInclusive, item.DelayExclusive));
                await sentTask();
            });
        }
    }
}
