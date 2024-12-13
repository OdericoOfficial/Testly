using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduledNodeGrain<TCommand> : IGrainWithGuidKey
        where TCommand : IModifyNodeCommand
    {
        Task ModifyAsync(TCommand item);

        Task ClearAsync();
    }
}
