using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IModifyCommandAsyncHandler<TModifyCommand> : IGrainWithGuidKey
        where TModifyCommand : ModifyScheduledNodeCommand
    {
        Task ModifyAsync(TModifyCommand item);
    }
}