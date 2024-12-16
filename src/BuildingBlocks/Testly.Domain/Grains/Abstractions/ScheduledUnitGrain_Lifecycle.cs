using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Attributes;
using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract partial class ScheduledUnitGrain<TSentEvent, TRequest, TCommand> : ScheduledNodeGrain<TCommand, ScheduledUnitState<TCommand>> 
        where TSentEvent : SentEvent
        where TCommand : ModifyScheduledUnitCommand
    {
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _sentFactory;
        private IAsyncStream<MeasurementUnitModifyEvent>? _measurementUnitModifyStream;
        private IAsyncStream<MeasurementUnitCancelEvent>? _measurementUnitCancelStream;

        private IAsyncStream<MeasurementUnitModifyEvent> MeasurementUnitModifyStream
            => _measurementUnitModifyStream ??= StreamProvider.GetStream<MeasurementUnitModifyEvent>(NodeId);

        private IAsyncStream<MeasurementUnitCancelEvent> MeasurementUnitCancelStream
            => _measurementUnitCancelStream ??= StreamProvider.GetStream<MeasurementUnitCancelEvent>(NodeId);

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
