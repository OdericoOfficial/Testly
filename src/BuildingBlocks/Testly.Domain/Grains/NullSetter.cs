using Orleans.Streams;
using TorchSharp;

namespace Testly.Domain.Grains
{
    internal static class NullSetter
    {
        public static Task UnsubscribeSetNullAsync<T>(ref StreamSubscriptionHandle<T>? handle)
        {
            var tempHandle = handle;
            handle = null;
            return tempHandle is not null ? tempHandle.UnsubscribeAsync() : Task.CompletedTask;
        }

        public static void DisposeTensorSetNull(ref torch.Tensor? tensor)
        {
            var tempTensor = tensor;
            tensor = null;
            tempTensor?.Dispose();
        }
    }
}
