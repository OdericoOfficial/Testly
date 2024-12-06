using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Commands;
using Testly.Domain.Events;
using Testly.Domain.Factories.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class ScheduleGrain<TSentEvent, TRequest, TCommand> : Grain, IScheduleGrain<TCommand>
        where TSentEvent : SentEvent
        where TCommand : ScheduleCommand
    {
        private readonly IScheduleSessionFactory<TRequest, TCommand> _sessionFactory;
        private readonly ISchduleSentEventFactory<TSentEvent, TRequest> _eventFactory;
        private readonly ILogger<ScheduleGrain<TSentEvent, TRequest, TCommand>> _logger;

        private IStreamProvider? _streamProvider;

        protected ScheduleGrain(IScheduleSessionFactory<TRequest, TCommand> sessionFactory, ISchduleSentEventFactory<TSentEvent, TRequest> eventFactory,
            ILogger<ScheduleGrain<TSentEvent, TRequest, TCommand>> logger)
        {
            _sessionFactory = sessionFactory;
            _eventFactory = eventFactory;
            _logger = logger;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public async Task RunAsync(TCommand command)
        {
            var aggregateId = this.GetPrimaryKey();
            var aggregateGrain = GrainFactory.GetGrain<IAggregateGrain>(aggregateId);
            await aggregateGrain.StartMeasurementAsync(command.Sample, command.BatchSize);

            await InternalScheduleAsync(command, aggregateId, RunSessionAsync);
        }

        protected abstract Task InternalScheduleAsync(TCommand command, Guid aggregateId, Func<TCommand, Guid, Task> sessionTask);

        [Rougamo<LoggingException>]
        private async Task RunSessionAsync(TCommand command, Guid aggregateId)
        {
            var request = _sessionFactory.Create(command, aggregateId);

            var tuple = await _sessionFactory.CreateAsyncInvoker()
                .Invoke(command, request);
            var sentEvent = await _eventFactory.CreateAsync(request, tuple, aggregateId);

            var isDispose = false;

            if (request is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
                isDispose = true;
            }

            if (!isDispose && request is IDisposable disposable)
                disposable.Dispose();

            if (_streamProvider is not null)
            {
                var sentStream = _streamProvider.GetStream<TSentEvent>(Constants.DefaultSessionValidatorNamespace, sentEvent.ValidatorId);
                await sentStream.OnNextAsync(sentEvent);
            }
        }
    }
}
