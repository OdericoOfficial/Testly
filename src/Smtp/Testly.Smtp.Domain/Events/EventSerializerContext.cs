using System.Text.Json.Serialization;

namespace Testly.Smtp.Domain.Events
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(SmtpSentEvent))]
    [JsonSerializable(typeof(SmtpReceivedEvent))]
    [JsonSerializable(typeof(SmtpPayloadEvent))]
    public partial class EventSerializerContext : JsonSerializerContext
    {
    }
}
