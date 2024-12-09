namespace Testly.Domain.Commands.Abstractions
{
    public interface IScheduleUnitCommand
    {
        string Name { get; init; }

        string Description { get; init; }

        Guid GroupId { get; init; }

        Guid LayerId { get; init; }

        int Sample { get; init; }

        int BatchSize { get; init; }
    }
}
