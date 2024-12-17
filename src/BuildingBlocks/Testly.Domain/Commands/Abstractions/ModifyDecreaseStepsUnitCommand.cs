namespace Testly.Domain.Commands.Abstractions
{
    public abstract record ModifyDecreaseStepsUnitCommand : ModifyScheduledUnitCommand
    {
        public int Step { get; init; }

        public int DelayInclusive { get; init; }

        public int DelayExclusive { get; init; }
    }
}
