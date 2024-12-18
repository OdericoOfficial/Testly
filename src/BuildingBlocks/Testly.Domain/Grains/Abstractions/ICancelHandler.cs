namespace Testly.Domain.Grains.Abstractions
{
    public interface ICancelHandler : IGrainWithGuidKey
    {
        Task CancelAsync();
    }
}
