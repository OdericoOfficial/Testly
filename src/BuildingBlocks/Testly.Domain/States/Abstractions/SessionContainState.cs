namespace Testly.Domain.States.Abstractions
{
    public enum SessionContainState : byte
    {
        None = 0b_000,
        ContainSentEvent = 0b_001,
        ContainReceivedEvent = 0b_010,
        BothContained = 0b_100
    }
}
