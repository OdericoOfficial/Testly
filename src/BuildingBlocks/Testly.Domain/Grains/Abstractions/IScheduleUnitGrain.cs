﻿using Testly.Domain.Commands.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface IScheduleUnitGrain<TCommand> : IGrainWithGuidKey
        where TCommand : IModifyUnitCommand
    {
        Task ModifyAsync(TCommand command);

        Task ClearAsync();

        Task ExecuteAsync();
    }
}
