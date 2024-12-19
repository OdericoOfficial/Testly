using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Testly.Domain.Attributes;
using Testly.Domain.Commands;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Policies
{
    [Singleton<ISentPolicy<SerialCommand>>, Policy]
    internal sealed class SerialPolicy : ISentPolicy<SerialCommand>
    {
        public async Task ScheduleAsync(SerialCommand item, Func<Task> sentTask)
        {
            var count = item.Sample / item.BatchSize;
            var rest = item.Sample % item.BatchSize;
            for (var i = 0; i < count; i++)
            {
                await Parallel.ForAsync(0, item.BatchSize, async (index, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(RandomNumberGenerator.GetInt32(item.DelayInclusive, item.DelayExclusive));
                    await sentTask();
                });
            }

            if (rest > 0)
            {
                await Parallel.ForAsync(0, rest, async (index, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(RandomNumberGenerator.GetInt32(item.DelayInclusive, item.DelayExclusive));
                    await sentTask();
                });
            }
        }
    }
}
