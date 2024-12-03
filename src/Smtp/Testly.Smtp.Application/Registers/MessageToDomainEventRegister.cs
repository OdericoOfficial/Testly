using Mapster;
using MimeKit;
using Testly.DependencyInjection;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Application.Registers
{
    [Singleton<IRegister>]
    internal class MessageToDomainEventRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MimeMessage, SmtpSentDomainEvent>()
                .IgnoreNonMapped(true)
                .AfterMapping((message, domainEvent) =>
                {
                    domainEvent.From = (message.From.FirstOrDefault() as MailboxAddress)?.Address;
                    domainEvent.To = (message.To.FirstOrDefault() as MailboxAddress)?.Address;
                    domainEvent.Subject = message.Subject;
                    domainEvent.SentTime = DateTime.UtcNow;
                });

            config.NewConfig<MimeMessage, SmtpReceivedDomainEvent>()
                .IgnoreNonMapped(true)
                .AfterMapping((message, domainEvent) =>
                {
                    domainEvent.From = (message.From.FirstOrDefault() as MailboxAddress)?.Address;
                    domainEvent.To = (message.To.FirstOrDefault() as MailboxAddress)?.Address;
                    domainEvent.Subject = message.Subject;
                    domainEvent.ReceivedTime = DateTime.UtcNow;
                });
        }
    }
}
