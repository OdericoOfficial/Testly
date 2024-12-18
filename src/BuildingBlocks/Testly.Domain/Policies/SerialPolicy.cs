using System.Security.Cryptography;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Policies
{
    internal class SerialPolicy<TCommand> : ISerialPolicy<TCommand>
        where TCommand : ISerialCommand
    {
        public async Task ScheduleAsync(TCommand item, Func<Task> sentTask)
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
