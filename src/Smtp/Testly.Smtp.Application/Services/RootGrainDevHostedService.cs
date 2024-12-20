using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testly.Domain.Commands;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Grains.Abstractions;

namespace Testly.Smtp.Application.Services
{
#if DEBUG
    [HostedService]
#endif
    internal class RootGrainDevHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IGuidFactory _factory;
        private readonly IClusterClient _clusterClient;

        public RootGrainDevHostedService(ILogger<RootGrainDevHostedService> logger, IGuidFactory factory, IClusterClient clusterClient)
        {
            _logger = logger;
            _factory = factory;
            _clusterClient = clusterClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogCritical("Debug start.");
            var list = new List<Guid>();
            for (var i = 0; i < 10; i++)
                list.Add(await _factory.NextAsync());

            var root = _clusterClient.GetGrain<IModifyHandler<RootCommand>>(list[0]);
            await root.ModifyAsync(new RootCommand
            {
                Name = "Debug Root"
            });

            var v1 = _clusterClient.GetGrain<IModifyHandler<VerticalCommand>>(list[1]);
            await v1.ModifyAsync(new VerticalCommand
            {
                Name = "v1",
                Parent = list[0],
                Root = list[0]
            });

            var v2 = _clusterClient.GetGrain<IModifyHandler<VerticalCommand>>(list[2]);
            await v2.ModifyAsync(new VerticalCommand
            {
                Name = "v2",
                Parent = list[0],
                Root = list[0]
            });

            var serial1 = _clusterClient.GetGrain<IModifyHandler<SerialCommand>>(list[3]);
            await serial1.ModifyAsync(new SerialCommand
            {
                Name = "Serial",
                Parent = list[1],
                Root = list[0],
                Sample = 100000,
                BatchSize = 1000,
                DelayInclusive = 10,
                DelayExclusive = 100
            });

            //var spikes1 = _clusterClient.GetGrain<IModifyHandler<SpikesCommand>>(list[4]);
            //await spikes1.ModifyAsync(new SpikesCommand
            //{
            //    Name = "Spikes",
            //    Parent = list[2],
            //    Root = list[0],
            //    Sample = 10000,
            //    BatchSize = 1000,
            //    DelayInclusive = 10,
            //    DelayExclusive = 100
            //});

            var serial2 = _clusterClient.GetGrain<IModifyHandler<SerialCommand>>(list[5]);
            await serial2.ModifyAsync(new SerialCommand
            {
                Name = "Seria2",
                Parent = list[2],
                Root = list[0],
                Sample = 100000,
                BatchSize = 1000,
                DelayInclusive = 10,
                DelayExclusive = 100
            });

            //var spikes2 = _clusterClient.GetGrain<IModifyHandler<SpikesCommand>>(list[6]);
            //await spikes2.ModifyAsync(new SpikesCommand
            //{
            //    Name = "Spikes",
            //    Parent = list[2],
            //    Root = list[0],
            //    Sample = 10000,
            //    BatchSize = 1000,
            //    DelayInclusive = 10,
            //    DelayExclusive = 100
            //});

            var executor = _clusterClient.GetGrain<IExecuteHandler>(list[0]);
            await executor.ExecuteAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
