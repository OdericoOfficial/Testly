namespace Testly.Domain.Commands.Abstractions
{
    public interface INodeCommand
    {
        string Name { get; init; }

        string Description { get; init; }

        Guid Parent { get; init; }

        Guid Root { get; init; }

        bool IsParallel { get; init; }
    }
}
