using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.Domain.Attributes;
using Testly.Domain.Events;
using Testly.Domain.Observers.Abstractions;
using TorchSharp.Modules;

namespace Testly.Domain.Grains
{
    [GrainWithGuidKey]
    [StreamProvider]
    internal sealed partial class StorageGrain : Grain, 
        IEventObserver<ScalarEvent>, 
        IEventObserver<SummaryEvent>
    {
        private readonly ILogger _logger;
        private readonly SummaryWriter _writer;
        
        public StorageGrain(ILogger<StorageGrain> logger, SummaryWriter writer)
        {
            _logger = logger;
            _writer = writer;
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{GrainName} {GrainId} Deactivate: {DeactivateReason}",
                nameof(StorageGrain), this.GetPrimaryKey(), reason);
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public Task OnNextAsync(ScalarEvent item)
        {
            _writer.add_scalar($"{item.UnitName}-Avg", item.Avg, item.Index);
            _writer.add_scalar($"{item.UnitName}-Min", item.Min, item.Index);
            _writer.add_scalar($"{item.UnitName}-Max", item.Max, item.Index);
            _writer.add_scalar($"{item.UnitName}-Mid", item.Mid, item.Index);
            _writer.add_scalar($"{item.UnitName}-Std", item.Std, item.Index);
            return Task.CompletedTask;
        }

        [Rougamo<LoggingException>]
        public Task OnNextAsync(SummaryEvent item)
        {
            _writer.add_text(item.UnitName, $"Sample: {item.Sample}", 0);
            _writer.add_text(item.UnitName, $"ReceivedSample: {item.ReceivedSample}", 0);
            _writer.add_text(item.UnitName, $"Avg: {item.Avg}", 1);
            _writer.add_text(item.UnitName, $"Mid: {item.Mid}", 2);
            _writer.add_text(item.UnitName, $"Min: {item.Min}", 3);
            _writer.add_text(item.UnitName, $"Max: {item.Max}", 4);
            _writer.add_text(item.UnitName, $"Std: {item.Std}", 5);
            _writer.add_text(item.UnitName, $"Error: {item.Error}", 6);
            _writer.add_text(item.UnitName, $"TPS: {item.TPS}", 7);
            _writer.add_text(item.UnitName, $"90 Quantile: {item.Quantile90}", 8);
            _writer.add_text(item.UnitName, $"95 Quantile: {item.Quantile95}", 9);
            _writer.add_text(item.UnitName, $"99 Quantile: {item.Quantile99}", 10);
            return Task.CompletedTask;
        }
    }
}
