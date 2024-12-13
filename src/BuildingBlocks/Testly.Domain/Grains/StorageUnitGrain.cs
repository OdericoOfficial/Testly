using Testly.Domain.Events;

namespace Testly.Domain.Grains
{
    internal partial class StorageUnitGrain
    {
        public async Task OnNextAsync(ScalarEvent item)
        {
            await _scalarRepository.AddAsync(item);
            _unitId = item.PublisherId;
        }

        public async Task OnNextAsync(SummaryEvent item)
        {
            await _summaryRepository.AddAsync(item);
            _unitId = item.PublisherId;
        }
    }
}
