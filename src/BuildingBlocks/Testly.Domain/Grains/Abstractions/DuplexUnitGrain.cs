using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    [OverrideHandle]
    public abstract partial class DuplexUnitGrain<TSentEvent, TReceivedEvent, TRequest, TCommand> : SimplexUnitGrain<TSentEvent, TRequest, TCommand>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
        where TCommand : IUnitCommand
    {
        [SubscribeAsyncStream]
        private IAsyncStream<TReceivedEvent>? _tReceivedEventStream;

        protected DuplexUnitGrain(ILogger logger, 
            ISentPolicy<TCommand> policy, 
            ISentFactory<TRequest, TCommand> sentFactory, 
            ISentEventFactory<TSentEvent, TRequest> sentEventFactory) : base(logger, policy, sentFactory, sentEventFactory)
        {
        }
    }
}