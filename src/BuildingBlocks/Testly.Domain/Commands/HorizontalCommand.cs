using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Commands
{
    [GenerateSerializer(IncludePrimaryConstructorParameters = false)]
    public sealed record HorizontalCommand : INodeCommand
    {
        [Id(0)]
        public string Name { get; init; } = string.Empty;

        [Id(1)]
        public string Description { get; init; } = string.Empty;

        [Id(2)]
        public Guid Parent { get; init; }

        [Id(3)]
        public Guid Root { get; init; }
    }
}
