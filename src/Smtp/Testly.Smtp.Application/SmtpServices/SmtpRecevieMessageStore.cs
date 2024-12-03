using MapsterMapper;
using MimeKit;
using Orleans;
using SmtpServer.Storage;
using SmtpServer;
using System.Buffers;
using Testly.DependencyInjection;
using Testly.Smtp.Domain.Events;
using SmtpServer.Protocol;
using Microsoft.Extensions.Logging;
using Orleans.GrainDirectory;

namespace Testly.Smtp.Application.SmtpServices
{
    [Transient<IMessageStore>]
    internal class SmtpRecevieMessageStore : MessageStore
    {
        private readonly IClusterClient _clusterClient;
        private readonly ILogger<SmtpRecevieMessageStore> _logger;

        public SmtpRecevieMessageStore(IClusterClient clusterClient, ILogger<SmtpRecevieMessageStore> logger)
        {
            _clusterClient = clusterClient;
            _logger = logger;
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction,
            ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                await using var stream = new MemoryStream();

                var position = buffer.GetPosition(0);
                while (buffer.TryGet(ref position, out var memory))
                    await stream.WriteAsync(memory, cancellationToken);
                stream.Position = 0;

                using var message = await MimeMessage.LoadAsync(stream, cancellationToken);
                if (!Guid.TryParse(message.TextBody, out var grainId))
                    return SmtpResponse.BadSequence;


                return SmtpResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return SmtpResponse.BadSequence;
            }
        }
    }

}
