using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IModifyHandler<TCommand> : IGrainWithGuidKey
        where TCommand : INodeCommand
    {
        Task ModifyAsync(TCommand item);
    }
}