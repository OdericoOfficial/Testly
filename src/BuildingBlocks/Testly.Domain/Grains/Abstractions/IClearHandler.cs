namespace Testly.Domain.Grains.Abstractions
{
    public interface IClearHandler : IGrainWithGuidKey
    {
        Task ClearAsync();
    }
}
