namespace Testly.Domain.Commands.Abstractions
{
    public interface IIncreaseCommand : IUnitCommand
    {
        int Step { get; init; }
    }
}
