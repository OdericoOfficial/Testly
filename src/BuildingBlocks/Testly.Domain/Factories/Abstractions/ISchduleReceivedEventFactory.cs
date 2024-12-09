using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Factories.Abstractions
{
    public interface ISchduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        TReceivedEvent Create(TResponse response, TSentEvent sentEvent, DateTime receivedTime, int receivedIndex);
    }
}
