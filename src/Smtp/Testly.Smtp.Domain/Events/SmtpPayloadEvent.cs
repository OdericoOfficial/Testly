namespace Testly.Smtp.Domain.Events
{
    [Serializable]
    public record SmtpPayloadEvent
    {
        public string From { get; init; } = string.Empty;

        public string To { get; init; } = string.Empty;

        public string Subject { get; init; } = string.Empty;

        public Guid UnitId {  get; init; }
    }
}
