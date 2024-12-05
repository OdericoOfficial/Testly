using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MimeKit;
using Testly.Domain.Factories;
using Testly.Smtp.Domain.Commands;

namespace Testly.Smtp.Application.Factories
{
    internal class SmtpSessionFactory<TSmtpScheduleCommand> : IScheduleSessionFactory<MimeMessage, TSmtpScheduleCommand>
        where TSmtpScheduleCommand : SmtpScheduleCommand
    {
        public MimeMessage Create(TSmtpScheduleCommand command, Guid aggregateId, int sentIndex)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(command.CertainFrom = command.CertainFrom ?? GetRandomEmailAddress(command)));
            message.To.Add(MailboxAddress.Parse(command.CertainTo = command.CertainTo ?? GetRandomEmailAddress(command)));
            message.Subject = command.CertainSubject = command.CertainSubject ?? GetRandomString();
            message.Body = new TextPart("plain")
            {
                Text = $"{aggregateId}|{sentIndex}"
            };
            return message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Func<TSmtpScheduleCommand, MimeMessage, Task<(DateTime SendingTime, DateTime SentTime)>> GetAsyncInvoker()
            => SendAsync;

        private async Task<(DateTime SendingTime, DateTime SentTime)> SendAsync(TSmtpScheduleCommand command, MimeMessage request)
        {
            var sendingTime = DateTime.UtcNow;
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(command.MTAAddress, command.MTAPort);
                if (command.LoginTuple.HasValue)
                    await client.AuthenticateAsync(command.LoginTuple.Value.Username, command.LoginTuple.Value.Password);   
                await client.SendAsync(request);
                await client.DisconnectAsync(true);
            }
            var sentTime = DateTime.UtcNow;
            return (sendingTime, sentTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetRandomEmailAddress(TSmtpScheduleCommand command)
            => $"{GetRandomString()}@{command.EmailDomains[RandomNumberGenerator.GetInt32(command.EmailDomains.Length)]}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetRandomString()
            => RandomNumberGenerator.GetString("qazwsxedcrfvtgbyhnujmikolp1234567890", 10);
    }
}
