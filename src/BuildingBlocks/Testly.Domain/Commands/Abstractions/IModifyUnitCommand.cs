namespace Testly.Domain.Commands.Abstractions
{
    public interface IModifyUnitCommand : IModifyNodeCommand
    {
        int Sample { get; init; }

        int BatchSize { get; init; }
    }
}
