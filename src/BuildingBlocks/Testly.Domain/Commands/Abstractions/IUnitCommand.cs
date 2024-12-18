namespace Testly.Domain.Commands.Abstractions
{
    public interface IUnitCommand : INodeCommand
    {
        int Sample { get; init; }

        int BatchSize { get; init; }

        int DelayInclusive { get; init; }

        int DelayExclusive { get; init; }
    }
}
