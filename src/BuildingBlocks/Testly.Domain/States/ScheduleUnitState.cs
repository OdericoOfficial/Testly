using Testly.Domain.Commands.Abstractions;
using Testly.Domain.Events;

namespace Testly.Domain.States
{
    public class ScheduleUnitState<TCommand>
        where TCommand : struct, IModifyUnitCommand
    {
        public TCommand Command { get; set; }

        public SummaryEvent Summary { get; set; }

        public List<ScalarEvent> Scalars { get; set; } = []; 

        public Guid CurrentAggregateId { get; set; }

        public int CompletedCount { get; set; }

        public int Process { get; set; }
    }
}
