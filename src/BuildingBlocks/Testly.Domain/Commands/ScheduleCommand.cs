namespace Testly.Domain.Commands
{
    public abstract class ScheduleCommand
    {
        public int Sample { get; set; }

        public int BatchSize { get; set; }
    }
}
