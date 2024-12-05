using System.Runtime.CompilerServices;
using Testly.Domain.Grains;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Grains
{
    internal class SmtpSessionValidatorGrain : SessionValidatorGrain<SmtpSentEvent, SmtpReceivedEvent>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool ValidateSession(SmtpSentEvent sentEvent, SmtpReceivedEvent receivedEvent)
            => sentEvent.From == receivedEvent.From
                && sentEvent.To == receivedEvent.To
                && sentEvent.Subject == receivedEvent.Subject;
    }
}
