﻿namespace Testly.Domain.Commands
{
    public record struct ModifyLayerCommand
    {
        public string Name { get; init; }

        public string Description { get; init; }

        public Guid GroupId { get; set; }
    }
}
