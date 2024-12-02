using System.Buffers;
using System.Text;
using AutoMapper;
using MimeKit;
using Orleans;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using Testly.DependencyInjection;

namespace Testly.Core.Smtp
{
    [Transient<IMessageStore>]
    internal class SmtpRecevieMessageStore : MessageStore
    {
        private readonly IMapper _mapper;
        private readonly IClusterClient _clusterClient;
        
        public SmtpRecevieMessageStore(IMapper mapper, IClusterClient clusterClient)
        {
            _mapper = mapper;
            _clusterClient = clusterClient;
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction,
            ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
                await stream.WriteAsync(memory, cancellationToken);
            stream.Position = 0;

            using var message = await MimeMessage.LoadAsync(stream, cancellationToken);
            var domainEvent = _mapper.Map<MimeMessage, SmtpReceivedDomainEvent>(message);

            var streamProvider = _clusterClient.GetStreamProvider("StreamProvider");
            

            return SmtpResponse.Ok;
        }
    }
}
