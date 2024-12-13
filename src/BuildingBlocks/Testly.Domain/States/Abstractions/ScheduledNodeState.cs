namespace Testly.Domain.States.Abstractions
{
    public enum ScheduledNodeState : byte
    {
        None = 0b_000,
        Executing = 0b_001,
        Completed = 0b_010,
        Cancelled = 0b_100
    }
}
