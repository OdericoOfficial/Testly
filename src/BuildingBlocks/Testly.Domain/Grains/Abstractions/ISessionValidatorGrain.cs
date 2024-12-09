﻿using Orleans.Streams;
using Testly.Domain.Events.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{
    public interface ISessionValidatorGrain<TSentEvent, TReceivedEvent> : IGrainWithGuidKey
        where TSentEvent : struct, ISentEvent
        where TReceivedEvent : struct, IReceivedEvent
    {
        Task OnSentEventReceivedAsync(TSentEvent sentEvent, StreamSequenceToken? sequenceToken = null);

        Task OnReceivedEventReceivedAsync(TReceivedEvent receivedEvent, StreamSequenceToken? sequenceToken = null);
    }
}