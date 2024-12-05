using Testly.Domain.Commands;
using Testly.Domain.Events;
using Testly.Domain.Factories;
using Testly.Domain.Grains;

namespace Testly.Application.Services
{
    public abstract class ScheduleService<TSentEvent, TReceivedEvent, TRequest, TResponse, TCommand> : IScheduleService<TCommand>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
        where TCommand : ScheduleCommand
    {
        private readonly IClusterClient _clusterClient;
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly IScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse> _eventFactory;

        protected ScheduleService(IClusterClient clusterClient, IScheduleSessionFactory<TRequest, TCommand> sessionFactory,
            IScheduleEventFactory<TSentEvent, TReceivedEvent, TRequest, TResponse> eventFactory)
        {
            _clusterClient = clusterClient;
            _sessionFactory = sessionFactory;
            _eventFactory = eventFactory;
        }

        public async Task RunAsync(TCommand command)
        {
            var aggreagateId = Guid.NewGuid();
            var aggregateGrain = _clusterClient.GetGrain<IAggregateGrain>(aggreagateId);
            await aggregateGrain.StartMeasurementAsync(command.Sample, command.BatchSize);
            await InternalScheduleAsync(command, aggreagateId, RunSessionAsync);
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Guid aggregateId, Func<TCommand, Guid, int, Task> sessionTask);

        private async Task RunSessionAsync(TCommand command, Guid aggregateId, int sentIndex)
        {
            var request = _sessionFactory.Create(command, aggregateId, sentIndex);

            var tuple = await _sessionFactory.GetAsyncInvoker()
                .Invoke(command, request);
            var sentEvent = _eventFactory.CreateSentEvent(request, tuple, aggregateId, sentIndex);

            var isDispose = false;

            if (request is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
                isDispose = true;
            }

            if (!isDispose && request is IDisposable disposable)
                disposable.Dispose();

            var sessionValidatorGrain = _clusterClient.GetGrain<ISessionValidatorGrain<TSentEvent, TReceivedEvent>>($"{aggregateId}|{sentIndex}");
            await sessionValidatorGrain.CacheSentEventAsync(sentEvent);
        }
    }
}
