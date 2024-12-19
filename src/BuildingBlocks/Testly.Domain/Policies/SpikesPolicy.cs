using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Testly.Domain.Attributes;
using Testly.Domain.Commands;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Policies
{
    [Singleton<ISentPolicy<SpikesCommand>>, Policy]
    internal sealed class SpikesPolicy : ISentPolicy<SpikesCommand>
    {
        public async Task ScheduleAsync(SpikesCommand item, Func<Task> sentTask)
        {
            await Parallel.ForAsync(0, item.Sample, async (index, cancellationToken) =>
            {
                await Task.Delay(RandomNumberGenerator.GetInt32(item.DelayInclusive, item.DelayExclusive));
                await sentTask();
            });
        }
    }
}
