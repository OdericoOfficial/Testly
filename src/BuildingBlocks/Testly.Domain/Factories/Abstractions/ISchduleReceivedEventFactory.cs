﻿using Testly.Domain.Events;

namespace Testly.Domain.Factories.Abstractions
{
    public interface ISchduleReceivedEventFactory<TSentEvent, TReceivedEvent, TResponse>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {
        TReceivedEvent Create(TResponse response, TSentEvent sentEvent, DateTime receivedTime, int receivedIndex);
    }
}
