﻿using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    public sealed class ChildNodeState
    {
        public NodeCurrentState CurrentState { get; set; } = NodeCurrentState.None;

        public Guid Child { get; set; }
    }
}
