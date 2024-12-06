using Testly.Domain.Commands;
using Testly.Domain.Grains.Abstractions;
using Testly.Domain.States;

namespace Testly.Domain.Grains
{
    internal class ScheduleGroupGrain : Grain<ScheduleGroupState>, IScheduleGroupGrain
    {
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

        public Task<ScheduleGroupState> GetGroupAsync()
        {
            throw new NotImplementedException();
        }
    }
}
