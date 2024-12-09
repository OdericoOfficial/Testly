using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;

namespace Testly.Domain.States
{
    public enum ScheduleUnitProcess
    {
        None = 0,
        Finished = 0,
        Running = 1,
    }

    public class ScheduleUnitState<TCommand>
        where TCommand : struct, IScheduleUnitCommand
    {
        public TCommand Command { get; set; }

        public SummaryEvent Summary { get; set; }

        public List<ScalarEvent> Scalars { get; set; } = []; 

        public int EventCount { get; set; }
    }
}
