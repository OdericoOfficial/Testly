using System.Text.Json;
using Microsoft.Extensions.Logging;
using Testly.Domain.Grains.Abstractions;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Grains
{
    internal sealed class SmtpDuplexValidatorGrain : DuplexValidatorGrain<SmtpSentEvent, SmtpReceivedEvent>
    {
        public SmtpDuplexValidatorGrain(ILogger<SmtpDuplexValidatorGrain> logger) : base(logger)
        {
        }

        protected override bool ValidateSession(SmtpSentEvent sentEvent, SmtpReceivedEvent receivedEvent)
        {
            if (sentEvent.From == receivedEvent.From
                && sentEvent.To == receivedEvent.To
                && sentEvent.Subject == receivedEvent.Subject)
            {
                var sentPayload = JsonSerializer.Deserialize(sentEvent.TextBody, EventSerializerContext.Default.SmtpPayloadEvent);
                var receivedPayload = JsonSerializer.Deserialize(receivedEvent.TextBody, EventSerializerContext.Default.SmtpPayloadEvent);
                return sentPayload is not null
                    && receivedPayload is not null
                    && sentPayload.Subject == receivedPayload.Subject
                    && sentPayload.From == receivedPayload.From
                    && sentPayload.To == receivedPayload.To;
            }
            return false;
        }
    }
}
