namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics.Models;

using Newtonsoft.Json;

internal class UsagePoint
{
    [JsonProperty("time")]
    public DateTime Time { get; set; }

    [JsonProperty("value")]
    public decimal? Value { get; set; }
}