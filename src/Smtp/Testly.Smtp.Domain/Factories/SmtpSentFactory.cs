using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using MimeKit;
using MimeKit.Text;
using Testly.Domain.Factories.Abstractions;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Domain.Factories
{
    [Singleton<ISentFactory<MimeMessage>>]
    internal class SmtpSentFactory : ISentFactory<MimeMessage>
    {
        private readonly ILogger<SmtpSentFactory> _logger;
        private readonly ObjectPool<SmtpClient> _clientPool;
        private readonly string _senderName;
        private readonly int _senderPort;
        private readonly ImmutableArray<string> _domains = ["qq.com", "foxmail.com", "163.com", "outlook.com", "gmail.com"];

        public SmtpSentFactory(ILogger<SmtpSentFactory> logger, ObjectPool<SmtpClient> clientPool, IConfiguration configuration)
        {
            _logger = logger;
            _clientPool = clientPool;
            _senderName = configuration["SmtpServer:SenderPort:SenderName"]!;
            _senderPort = Convert.ToInt32(configuration["SmtpServer:SenderPort"]);
        }

        public MimeMessage Create(Guid unitId)
        {
            var payload = new SmtpPayloadEvent
            {
                From = GetRandomEmailAddress(),
                To = GetRandomEmailAddress(),
                Subject = GetRandomString(),
                UnitId = unitId
            };

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(payload.From));
            message.To.Add(MailboxAddress.Parse(payload.To));
            message.Subject = payload.Subject;
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = JsonSerializer.Serialize(payload, EventSerializerContext.Default.SmtpPayloadEvent)
            };
            return message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetRandomEmailAddress()
            => $"{GetRandomString()}@{_domains[RandomNumberGenerator.GetInt32(_domains.Length)]}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetRandomString()
            => RandomNumberGenerator.GetString("qazwsxedcrfvtgbyhnujmikolp1234567890", 10);

        public Func<MimeMessage, Task<(DateTime SendingTime, DateTime SentTime)>> CreateAsyncInvoker()
            => InvokeAsync;

        private async Task<(DateTime SendingTime, DateTime SentTime)> InvokeAsync(MimeMessage message)
        {
            var sendingTime = DateTime.UtcNow;
            var client = _clientPool.Get();
            try
            {
                client.RequireTLS = true;
                await client.ConnectAsync(_senderName, _senderPort, false);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in SmtpSentFactory");
            }
            finally
            {
                _clientPool.Return(client);
            }
            var sendTime = DateTime.UtcNow;
            return (sendingTime, sendingTime);
        }
    }
}
