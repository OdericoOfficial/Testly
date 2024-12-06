using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MimeKit;
using Orleans.Streams;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using Testly.DependencyInjection;
using Testly.Domain.Factories;
using Testly.Domain.Grains.Abstractions;
using Testly.Smtp.Domain.Events;

namespace Testly.Smtp.Application.Services
{
    [Singleton<IMessageStore>]
    internal class SmtpMessageStore : MessageStore
    {
        private readonly IScheduleEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage> _eventFactory;
        private readonly IClusterClient _clusterClient;
        private readonly ILogger<SmtpMessageStore> _logger;

        public SmtpMessageStore(IScheduleEventFactory<SmtpSentEvent, SmtpReceivedEvent, MimeMessage, MimeMessage> eventFactory,
            IClusterClient clusterClient, ILogger<SmtpMessageStore> logger)
        {
            _eventFactory = eventFactory;
            _clusterClient = clusterClient;
            _logger = logger;
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, 
            ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                var receivedTime = DateTime.UtcNow;
                await using var stream = new MemoryStream();

                var position = buffer.GetPosition(0);
                while (buffer.TryGet(ref position, out var memory))
                    await stream.WriteAsync(memory, cancellationToken);
                stream.Position = 0;

                var message = await MimeMessage.LoadAsync(stream, cancellationToken);
                

                var receivedEvent = _eventFactory.CreateReceivedEvent(message, sentEvent, receivedTime, 0);
                var sessionValidatorGrain = _clusterClient.GetGrain<ISessionValidatorGrain<SmtpSentEvent, SmtpReceivedEvent>>($"{sentEvent.AggregateId}|{sentEvent.SentIndex}");
                await sessionValidatorGrain.CacheReceivedEventAsync(receivedEvent);
                return SmtpResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception {ServiceName}", nameof(SmtpMessageStore));
                return SmtpResponse.BadSequence;
            }
        }
    }
}
