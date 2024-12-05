using MimeKit;
using Testly.Application.Services;
using Testly.DependencyInjection;
using Testly.Domain.Factories;
using Testly.Smtp.Domain.Commands;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Services
{
    [Singleton<IScheduleService<SerialSchduleComand>>]
    internal class SerialScheduleService : ScheduleService<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage, SerialSchduleComand>
    {
        public SerialScheduleService(IClusterClient clusterClient, IScheduleSessionFactory<MimeMessage, SerialSchduleComand> sessionFactory, 
            IScheduleEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage> eventFactory) : base(clusterClient, sessionFactory, eventFactory)
        {
        }

        protected override async Task InternalScheduleAsync(SerialSchduleComand command, Guid aggregateId,
            Func<SerialSchduleComand, Guid, int, Task> sessionTask)
        {
            for (var i = 0; i < command.Epoch; i++)
                await Parallel.ForAsync(0, command.Sample / command.Epoch, async (sentIndex, _) =>
                    await sessionTask.Invoke(command, aggregateId, sentIndex));
        }
    }
}
