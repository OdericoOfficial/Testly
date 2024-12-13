using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.States.Abstractions
{
    public interface ISentState<TSentEvent>
        where TSentEvent : struct, ISentEvent
    {
        TSentEvent SentEvent { get; }

        byte ContainState { get; }

        void ApplyNoneToContainSentEvent(TSentEvent item);
    }
}
