namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics.Models;

using Newtonsoft.Json;

internal class BandwidthDetails
{
    [JsonProperty("site_code")]
    public string SiteCode { get; set; }

    [JsonProperty("host_name")]
    public string Host { get; set; }

    [JsonProperty("wan_bw")]
    public long? WANBandwidth { get; set; }

    [JsonProperty("int_bw")]
    public long? INTBandwidth { get; set; }
}
