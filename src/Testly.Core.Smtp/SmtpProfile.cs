using AutoMapper;
using MimeKit;

namespace Testly.Core.Smtp
{
    internal class SmtpProfile : Profile
    {
        public SmtpProfile()
        {
            CreateMap<MimeMessage, SmtpReceivedDomainEvent>()
                .ConstructUsing(message => new SmtpReceivedDomainEvent { });
        }
    }
}
