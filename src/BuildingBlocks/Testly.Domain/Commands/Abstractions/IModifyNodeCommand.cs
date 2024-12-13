namespace Testly.Domain.Commands.Abstractions
{
    public interface IModifyNodeCommand
    {
        string Name { get; init; }

        string Description { get; init; }

        Guid LastId { get; init; }
    }
}
