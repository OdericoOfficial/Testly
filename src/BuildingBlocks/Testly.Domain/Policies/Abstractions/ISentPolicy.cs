using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Policies.Abstractions
{
    public interface ISentPolicy<TCommand>
        where TCommand : IUnitCommand
    {
        Task ScheduleAsync(TCommand item, Func<Task> sentTask);
    }
}
