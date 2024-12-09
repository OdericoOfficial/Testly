using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Commands;
using Testly.Domain.Events;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains
{
    [ImplicitStreamSubscription(Constants.DefaultSchduleUnitCompletedNamespace)]
    internal class ScheduleLayerGrain : Grain<ScheduleLayerState>, IScheduleLayerGrain
    {
        private readonly ILogger<ScheduleLayerGrain> _logger;

        private IStreamProvider? _streamProvider;
        private IAsyncStream<ScheduleUnitCompletedEvent>? _unitdStream;
        private StreamSubscriptionHandle<ScheduleUnitCompletedEvent>? _unitHandle;
        private bool _isModified;

        public ScheduleLayerGrain(ILogger<ScheduleLayerGrain> logger)
        {
            _logger = logger;
        }

        [Rougamo<LoggingException>]
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _isModified = false;
            _streamProvider = this.GetStreamProvider(Constants.DefaultStreamProvider);
            _unitdStream = _streamProvider.GetStream<ScheduleUnitCompletedEvent>(Constants.DefaultSchduleUnitCompletedNamespace, this.GetPrimaryKey());
            _unitHandle = await _unitdStream.SubscribeAsync(OnNextAsync, ex =>
            {
                _logger.LogError(ex, "Unexpected exception in AsyncStream {Name} from {GrainName} {GrainId}",
                    nameof(ScheduleUnitCompletedEvent), nameof(ScheduleLayerGrain), this.GetPrimaryKey());
                return Task.CompletedTask;
            });
        }

        [Rougamo<LoggingException>]
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (_isModified)
                await WriteStateAsync();

            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(AggregateGrain), this.GetPrimaryKey(), reason);
        }

        public Task AddLayerAsync(AddLayerCommand command)
        {
            _isModified = true;
            State.Command = command;
            return Task.CompletedTask;
        }

        public async Task DeleteLayerAsync()
        {
            foreach (var item in State.Units)
            {

            }

            await ClearStateAsync();
        }

        [Rougamo<LoggingException>]
        public Task OnNextAsync(ScheduleUnitCompletedEvent item, StreamSequenceToken? token = null)
        {
            var cache = State.Units.FirstOrDefault(unit => unit.ScheduleId == item.ScheduleId);
            if (cache is not null)
            {
                _isModified = true;
                cache.IsFinished = true;
                State.EventCount++;
                State.StartTime = State.StartTime > item.StartTime ? item.StartTime : State.StartTime;
                State.EndTime = State.EndTime < item.EndTime ? item.EndTime : State.EndTime;

                if (State.EventCount == State.Units.Count 
                    && _streamProvider is not null)
                {
                    var layerStream = _streamProvider.GetStream<ScheduleLayerCompletedEvent>(Constants.DefaultSchduleLayerCompletedNamesapce,
                        State.Command.GroupId);  
                    return layerStream.OnNextAsync(new ScheduleLayerCompletedEvent
                    {
                        StartTime = State.StartTime,
                        EndTime = State.EndTime,
                        LayerId = this.GetPrimaryKey()
                    });
                }
            }
            return Task.CompletedTask;
        }
    }
}
