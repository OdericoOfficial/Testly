using MapsterMapper;
using Microsoft.Extensions.Logging;
using Testly.Domain.Commands;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains
{
    internal class ScheduleGroupGrain : Grain<ScheduleGroupState>, IScheduleGroupGrain
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleGroupGrain> _logger;

        public ScheduleGroupGrain(IMapper mapper, ILogger<ScheduleGroupGrain> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public Task ModifyGroupAsync(ModifyGroupCommand command)
        {
            State.Subject = command.Subject;
            State.Description = command.Description;
            State.ModifiedTime = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task<Guid> AddLayerAsync(AddLayerCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
