using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Commands
{
    [GenerateSerializer(IncludePrimaryConstructorParameters = false)]
    public sealed record IncreaseCommand : IUnitCommand
    {
        [Id(0)]
        public string Name { get; init; } = string.Empty;

        [Id(1)]
        public string Description { get; init; } = string.Empty;

        [Id(2)]
        public Guid Parent { get; init; }

        [Id(3)]
        public Guid Root { get; init; }

        [Id(4)]
        public int Sample { get; init; }

        [Id(5)]
        public int BatchSize { get; init; }

        [Id(6)]
        public int DelayInclusive { get; init; }

        [Id(7)]
        public int DelayExclusive { get; init; }

        [Id(8)]
        public int Step { get; init; }
    }
}
