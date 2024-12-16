namespace Testly.Domain.Commands.Abstractions
{
    public abstract record ModifyScheduledNodeCommand
    {
        public required string Name { get; init; }

        public required string Description { get; init; }

        public Guid LastId { get; init; }
    }
}
