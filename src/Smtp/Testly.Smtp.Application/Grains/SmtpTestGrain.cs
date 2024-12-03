using MapsterMapper;
using Orleans.Streams;
using Testly.Application.Grains;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Application.Grains
{
    internal class SmtpTestGrain : TestableGrain<SmtpSentDomainEvent, SmtpReceivedDomainEvent>, ISmtpTestGrain
    {
        public SmtpTestGrain(IAsyncObserver<SmtpSentDomainEvent> sentEventObserver, IAsyncObserver<SmtpReceivedDomainEvent> receviedEventObserver, 
            IMapper mapper) : base(sentEventObserver, receviedEventObserver, mapper)
        {
        }

        protected override void AfterSentEventMapping(SmtpSentDomainEvent domainEvent)
        {

        }

        protected override void AfterReceivedEventMapping(SmtpReceivedDomainEvent domainEvent)
        {
        }
    }
}
