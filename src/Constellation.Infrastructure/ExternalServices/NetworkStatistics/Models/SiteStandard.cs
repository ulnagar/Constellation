namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics.Models;

using Newtonsoft.Json;

internal class SiteStandard
{
    [JsonProperty("st_id")]
    public int Id { get; set; }

    [JsonProperty("st_name")]
    public string Name { get; set; }
}
