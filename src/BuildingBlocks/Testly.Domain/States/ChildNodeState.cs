using Testly.Domain.States.Abstractions;

namespace Testly.Domain.States
{
    [Serializable]
    public sealed class ChildNodeState
    {
        public NodeCurrentState CurrentState { get; set; } = NodeCurrentState.None;

        public Guid Child { get; set; }
    }
}
