using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduledUnitGrain<TSentEvent, TRequest, TCommand> : ScheduledNodeGrain<TCommand, ScheduledUnitState<TCommand>>, 
        IScheduledUnitGrain<TCommand>
        where TSentEvent : ISentEvent
        where TCommand : IModifyUnitCommand
    {
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _sentFactory;
        private IAsyncStream<AggregateUnitCancelEvent>? _aggregateUnitCancelStream;

        private IAsyncStream<AggregateUnitCancelEvent> AggregateUnitCancelStream
            => _aggregateUnitCancelStream ??= StreamProvider.GetStream<AggregateUnitCancelEvent>(NodeId);

        protected ScheduledUnitGrain(ILogger logger, 
            IAsyncObserver<ScheduledNodeExecuteEvent> nodeExecuteObserver,
            IAsyncObserver<ScheduledNodeCancelEvent> nodeCancelObserver,
            IAsyncObserver<ScheduledNodeCompletedEvent> nodeCompletedObserver,
            IScheduleSessionFactory<TRequest, TCommand> sessionFactory, 
            ISchduleSentEventFactory<TSentEvent, TRequest> sentFactory) 
                : base(logger, nodeExecuteObserver, nodeCompletedObserver, nodeCancelObserver)
        {
            _sessionFactory = sessionFactory;
            _sentFactory = sentFactory;
        }
    }
}
