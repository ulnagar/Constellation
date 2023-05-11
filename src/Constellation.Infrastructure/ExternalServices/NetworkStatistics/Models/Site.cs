namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics.Models;

using Newtonsoft.Json;

internal class Site
{
    [JsonProperty("site_id")]
    public int Id { get; set; }

    [JsonProperty("site_router")]
    public string RouterName { get; set; }

    [JsonProperty("site_name")]
    public string SiteName { get; set; }

    [JsonProperty("site_school_no")]
    public string SchoolCode { get; set; }

    [JsonProperty("site_bw")]
    public double? Bandwidth { get; set; }

    public long WANBandwidth { get; set; }
    public long INTBandwidth { get; set; }

    [JsonProperty("site_standard")]
    public ICollection<SiteStandard> Standards { get; set; }

    public string BandwidthDisplay { get; set; }

    public ICollection<PairedUsage> WANData { get; set; }
    // public ICollection<PairedUsage> NETData { get; set; }

    public Site()
    {
        Standards = new List<SiteStandard>();

        WANData = new List<PairedUsage>();
        //NETData = new List<PairedUsage>();
    }
}
