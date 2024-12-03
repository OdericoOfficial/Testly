using Testly.Application.Grains;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Application.Grains
{
    public interface ISmtpTestGrain : ITestableGrain<SmtpSentDomainEvent, SmtpReceivedDomainEvent>
    {
    }
}
