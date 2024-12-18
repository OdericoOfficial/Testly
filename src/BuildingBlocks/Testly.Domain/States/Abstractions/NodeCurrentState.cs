namespace Testly.Domain.States.Abstractions
{
    [Flags]
    public enum NodeCurrentState : byte
    {
        None = 0b_000000,
        Wrote = 0b_000001,
        Modified = 0b_000010,
        Executing = 0b_000100,
        Completed = 0b_001000,
        Cancelled = 0b_010000,
        Cleaned = 0b_100000
    }
}
