namespace Testly.Domain.Grains.Abstractions
{
    public interface IExecuteHandler : IGrainWithGuidKey
    {
        Task ExecuteAsync();
    }
}
