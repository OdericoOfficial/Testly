namespace Testly.Domain.Commands.Abstractions
{
    public abstract record ModifyScheduledUnitCommand : ModifyScheduledNodeCommand
    {
        public int Sample { get; init; }

        public int BatchSize { get; init; }
    }
}
