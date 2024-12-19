using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Application.Registers
{
    [Singleton<IRegister>, Enumerable]
    internal class MimeMessageRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MimeMessage, SmtpSentEvent>()
                .Map(item => item.From, item => item.From.First().Name)
                .Map(item => item.To, item => item.To.First().Name)
                .Map(item => item.Subject, item => item.Subject)
                .Map(item => item.TextBody, item => item.TextBody)
                .IgnoreNonMapped(true);


            config.NewConfig<MimeMessage, SmtpReceivedEvent>()
                .Map(item => item.From, item => item.From.First().Name)
                .Map(item => item.To, item => item.To.First().Name)
                .Map(item => item.Subject, item => item.Subject)
                .Map(item => item.TextBody, item => item.TextBody)
                .IgnoreNonMapped(true);
        }
    }
}
