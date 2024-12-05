using MimeKit;
using Testly.Application.Services;
using Testly.DependencyInjection;
using Testly.Domain.Factories;
using Testly.Smtp.Domain.Commands;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Services
{
    [Singleton<IScheduleService<StepsScheduleCommand>>]
    internal class StepsScheduleService : ScheduleService<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage, StepsScheduleCommand>
    {
        public StepsScheduleService(IClusterClient clusterClient, IScheduleSessionFactory<MimeMessage, StepsScheduleCommand> sessionFactory,
            IScheduleEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage> eventFactory) : base(clusterClient, sessionFactory, eventFactory)
        {
        }

        protected override async Task InternalScheduleAsync(StepsScheduleCommand command, Guid aggregateId,
            Func<StepsScheduleCommand, Guid, int, Task> sessionTask)
        {
            var sentIndex = 0;
            var outerEpoch = (command.Sample / command.InnerEpoch - command.Init) / command.Adder;
            var epochSample = command.Init;

            for (var i = 0; i < outerEpoch ; i++)
            {
                for (var j = 0; j < command.InnerEpoch; j++)
                {
                    _ = Parallel.ForAsync(0, epochSample, async (_, _) =>
                    {
                        await sessionTask.Invoke(command, aggregateId, sentIndex);
                        Interlocked.Increment(ref sentIndex);
                    });
                    await Task.Delay(command.Delay);
                }

                epochSample += command.Adder;
            }
        }
    }

}
