using Testly.Domain.Commands;

namespace Testly.Application.Services
{
    public interface IScheduleService<TCommand>
        where TCommand : ScheduleCommand
    {
        Task RunAsync(TCommand command);
    }
}
