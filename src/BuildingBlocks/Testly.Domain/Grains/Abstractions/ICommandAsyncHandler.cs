namespace Testly.Domain.Grains.Abstractions
{
    public interface ICommandAsyncHandler<TCommand> : IGrainWithGuidKey
    {
        Task HandleAsync(TCommand item);
    }
}
