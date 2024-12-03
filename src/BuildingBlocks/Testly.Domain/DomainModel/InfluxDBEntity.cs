using InfluxDB.Client.Core;

namespace Testly.Domain.DomainModel
{
    public abstract class InfluxDBEntity
    {
        [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }

        [Column] public string GrainKey { get; set; } = string.Empty;
    }
}
