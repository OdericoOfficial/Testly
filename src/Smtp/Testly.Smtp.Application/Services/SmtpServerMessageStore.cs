using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Orleans.Streams;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using Testly.Domain.Factories.Abstractions;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Application.Services
{
    [Singleton<IMessageStore>]
    internal class SmtpServerMessageStore : MessageStore
    {
        private readonly ILogger _logger;
        private readonly IStreamProvider _provider;
        private readonly IReceivedEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage> _factory;

        public SmtpServerMessageStore(ILogger<SmtpServerMessageStore> logger, 
            IClusterClient clusterClient, 
            IReceivedEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage> factory)
        {
            _logger = logger;
            _provider = clusterClient.GetStreamProvider("PubSubStore");
            _factory = factory;
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                var receivedTime = DateTime.UtcNow;
                await using var stream = new MemoryStream();

                var position = buffer.GetPosition(0);
                while (buffer.TryGet(ref position, out var memory))
                    stream.Write(memory.Span);

                stream.Position = 0;

                using var message = await MimeMessage.LoadAsync(stream, cancellationToken);
                if (!message.From.Any() || !message.To.Any() || message.Subject is null || message.TextBody is null)
                    return SmtpResponse.BadSequence;


                var sent = JsonSerializer.Deserialize(message.TextBody, EventSerializerContext.Default.SmtpSentEvent);
                if (sent is null)
                    return SmtpResponse.BadSequence;

                var received = _factory.Create(message, sent, receivedTime);
                var receivedEventStream = _provider.GetStream<SmtpReceivedEvent>(received.PublisherId);
                await receivedEventStream.OnNextAsync(received);

                return SmtpResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception catch in SmtpServerMessageStore.");
                return SmtpResponse.BadSequence;
            }
        }
    }
}
