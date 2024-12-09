using System.Security.Cryptography;
using MimeKit;
using Testly.DependencyInjection;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Grains.Abstractions;
using Testly.Smtp.Domain.Commands;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Services
{

    [Singleton<IScheduleUnitGrain<SpikesScheduleCommand>>]
    internal class SpikesScheduleService : ScheduleService<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage, SpikesScheduleCommand>
    {
        public SpikesScheduleService(IClusterClient clusterClient, IScheduleSessionFactory<MimeMessage, SpikesScheduleCommand> sessionFactory,
            IScheduleEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage> eventFactory) : base(clusterClient, sessionFactory, eventFactory)
        {
        }

        protected override Task InternalScheduleAsync(SpikesScheduleCommand command, Guid aggregateId,
        Func<SpikesScheduleCommand, Guid, int, Task> sessionTask)
            => Parallel.ForAsync(0, command.Sample, async (sentIndex, _) =>
            {
                await Task.Delay(RandomNumberGenerator.GetInt32(command.RandomDelayFrom, command.RandomDelayTo));
                await sessionTask.Invoke(command, aggregateId, sentIndex);
            });
    }
}
