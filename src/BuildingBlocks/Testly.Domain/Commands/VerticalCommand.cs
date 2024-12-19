using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Commands
{
    public sealed record VerticalCommand : INodeCommand
    {
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public Guid Parent { get; init; }

        public Guid Root { get; init; }
    }
}
