namespace Testly.Domain.Commands.Abstractions
{
    public abstract record ModifySerialUnitCommand : ModifyScheduledUnitCommand
    {
        public int DelayInclusive { get; init; }
        
        public int DelayExclusive { get; init; }
    }
}
