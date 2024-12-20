using Microsoft.Extensions.Logging;
using MimeKit;
using Testly.Domain.Commands;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies.Abstractions;
using Testly.Domain.Grains.Abstractions;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Grains
{
    [ImplicitStreamSubscription]
    internal sealed class SmtpDuplexSerialGrain : DuplexSerialGrain<SmtpSentEvent, SmtpReceivedEvent, MimeMessage>
    {
        public SmtpDuplexSerialGrain(ILogger<SmtpDuplexSerialGrain> logger,
            ISentPolicy<SerialCommand> policy,
            ISentFactory<MimeMessage> sentFactory,
            ISentEventFactory<SmtpSentEvent, MimeMessage> sentEventFactory) : base(logger, policy, sentFactory, sentEventFactory)
        {
        }
    }
}
