using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Commands;
using Testly.Domain.Events;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.States.Abstractions;

namespace Testly.Domain.Grains
{
    internal sealed class ScheduledVerticalGrain : ScheduledCollectionGrain<ModifyScheduledVerticalCommand>
    {
        public ScheduledVerticalGrain(ILogger<ScheduledVerticalGrain> logger) : base(logger)
        {
        }

        [Rougamo<LoggingException>]
        public override async Task OnNextAsync(ScheduledNodeExecuteEvent item)
        {
            if (State.CurrentState != ScheduledNodeCurrentState.Executing)
            {
                foreach (var id in State.Childs)
                {
                    var stream = StreamProvider.GetStream<ScheduledNodeExecuteEvent>(id);
                    await stream.OnNextAsync(new ScheduledNodeExecuteEvent
                    {
                        PublisherId = GrainId,
                        SubscriberId = id
                    });
                }
            }
            await base.OnNextAsync(item);
        }
    }
}
