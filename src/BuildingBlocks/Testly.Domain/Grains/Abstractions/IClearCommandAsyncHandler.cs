namespace Testly.Domain.Grains.Abstractions
{
    public interface IClearCommandAsyncHandler : IGrainWithGuidKey
    {
        Task ClearAsync();
    }
}
