namespace Testly.Domain.States.Abstractions
{
    public enum SentContainState : byte
    {
        None = 0b_000,
        ContainSentEvent = 0b_001
    }
}
