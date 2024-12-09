using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleUnitGrain<TCommand> : IGrainWithGuidKey
        where TCommand : IScheduleUnitCommand
    {
        Task AddUnitAsync(TCommand command);

        Task DeleteUnitAsync();

        Task RunAsync();
    }
}
