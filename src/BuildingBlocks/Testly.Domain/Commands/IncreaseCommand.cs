using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Commands
{
    public sealed record IncreaseCommand : IUnitCommand
    {
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public Guid Parent { get; init; }

        public Guid Root { get; init; }

        public int Sample { get; init; }

        public int BatchSize { get; init; }

        public int DelayInclusive { get; init; }

        public int DelayExclusive { get; init; }

        public int Step { get; init; }
    }
}
