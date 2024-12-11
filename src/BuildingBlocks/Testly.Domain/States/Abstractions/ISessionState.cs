using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.States.Abstractions
{
    public interface ISessionState<TSentEvent, TReceivedEvent> : ISentState<TSentEvent>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        TReceivedEvent ReceivedEvent { get; }

        void ApplyContainReceivedEventToBothContained(TSentEvent item);

        void ApplyNoneToContainReceivedEvent(TReceivedEvent item);

        void ApplyContainSentEventToBothContained(TReceivedEvent item);
    }
}
