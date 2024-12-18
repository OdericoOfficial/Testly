using Microsoft.Extensions.Logging;
using Testly.Domain.Commands;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Grains.Abstractions;

namespace Testly.Domain.Grains
{
    internal sealed class ParallelGrain : NodeGrain<ParallelCommand>
    {
        public ParallelGrain(ILogger<ParallelGrain> logger) : base(logger)
        {
        }

        protected override async Task InternaleExecuteAsync(NodeExecutingEvent item)
        {
            if (State.Children.Any() && NotifyExecutingEventStream is not null)
                await NotifyExecutingEventStream.OnNextBatchAsync(State.Children.Select(child => new NodeExecutingEvent
                {
                    PublisherId = GrainId,
                    SubscriberId = child.Child,
                    Mode = BatchMode.Parallel
                }));
            State.ApplyExecuting();
        }
    }
}
