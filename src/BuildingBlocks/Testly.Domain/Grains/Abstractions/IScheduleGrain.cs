using Testly.Domain.Commands;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleGrain<TCommand> : IGrainWithGuidKey
        where TCommand : ScheduleCommand
    {
        Task RunAsync(TCommand command);
    }
}
