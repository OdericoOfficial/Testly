using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Factories
{
    [Singleton<IGuidFactory>]
    internal class LocalSequentialGuidFactory : IGuidFactory
    {
        private long _counter = DateTime.UtcNow.Ticks;

        public ValueTask<Guid> NextAsync()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            var succeeded = Guid.NewGuid().TryWriteBytes(guidBytes);
            if (!succeeded)
                throw new UnreachableException("Could not write Guid to Span");

            var incrementedCounter = Interlocked.Increment(ref _counter);
            Span<byte> counterBytes = stackalloc byte[sizeof(long)];
            MemoryMarshal.Write(counterBytes, in incrementedCounter);

            if (!BitConverter.IsLittleEndian)
                counterBytes.Reverse();

            guidBytes[8] = counterBytes[1];
            guidBytes[9] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return ValueTask.FromResult(new Guid(guidBytes));
        }
    }
}
