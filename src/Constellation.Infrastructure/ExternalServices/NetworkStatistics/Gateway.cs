using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics
{
    public class Gateway : INetworkStatisticsGateway, IScopedService
    {
        private readonly HttpClient _client;
        private string _urlBase;


        public Gateway(INetworkStatisticsGatewayConfiguration settings)
        {
            _urlBase = settings.Url;

            _client = new HttpClient
            {
                //BaseAddress = new Uri(settings.Url)
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.Key);
        }

        private async Task<string> Request(string queryUri)
        {
            var response = await _client.GetAsync(queryUri);
            var resultStream = await response.Content.ReadAsStreamAsync();
            var sr = new StreamReader(resultStream);

            return sr.ReadToEnd();
        }

        public async Task<NetworkStatisticsSiteDto> GetSiteDetails(string schoolCode)
        {
            var queryUri = $"{_urlBase}/sites/{schoolCode}";

            var details = await Request(queryUri);
            var siteObject = JObject.Parse(details);
            IList<JToken> results = siteObject["items"].Children().ToList();

            IList<Site> sites = new List<Site>();
            foreach (var site in results)
            {
                var budSite = site.ToObject<Site>();
                budSite.BandwidthDisplay = BandwidthDisplayHelper.DecimalToFormattedString(Convert.ToDecimal(budSite.Bandwidth));
                sites.Add(budSite);
            }

            switch (sites.Count)
            {
                case 0:
                    return null;
                case 1:
                    return ConvertToDto(sites.First());
                case var expression when sites.Count > 1:
                    throw new DataException($"Too many sites returned!");
                default:
                    return null;
            }
        }

        public async Task GetSiteUsage(NetworkStatisticsSiteDto site, int day = 0)
        {
            var pageList = new List<PairedUsagePages>();
            var targetDate = DateTime.Today.AddDays(day);

            var queryUri = $"{_urlBase}/bandwidth/{site.SchoolCode}";

            var details = await Request(queryUri);

            var bwObject = JArray.Parse(details);
            IList<JToken> results = bwObject.Children().ToList();
            foreach (JToken detail in results)
            {
                var bwDetail = detail.ToObject<BandwidthDetails>();

                if (bwDetail.WANBandwidth.HasValue)
                    site.WANBandwidth = bwDetail.WANBandwidth.Value;

                if (bwDetail.INTBandwidth.HasValue)
                    site.INTBandwidth = bwDetail.INTBandwidth.Value;
            }

            var startTimestamp = targetDate.AddHours(7).GetUnixEpoch();
            var endTimestamp = targetDate.AddHours(16).GetUnixEpoch();

            if (site.INTBandwidth > 0)
            {
                var entry = new PairedUsagePages()
                {
                    WANInbound = $"{_urlBase}/metrics/inbound/wan/{site.SchoolCode}/from/{startTimestamp}/to/{endTimestamp}",
                    WANOutbound = $"{_urlBase}/metrics/outbound/wan/{site.SchoolCode}/from/{startTimestamp}/to/{endTimestamp}",
                    INTInbound = $"{_urlBase}/metrics/inbound/internet/{site.SchoolCode}/from/{startTimestamp}/to/{endTimestamp}",
                    INTOutbound = $"{_urlBase}/metrics/outbound/internet/{site.SchoolCode}/from/{startTimestamp}/to/{endTimestamp}"
                };

                pageList.Add(entry);
            }
            else
            {
                var entry = new PairedUsagePages()
                {
                    WANInbound = $"{_urlBase}/metrics/inbound/wan/{site.SchoolCode}/from/{startTimestamp}/to/{endTimestamp}",
                    WANOutbound = $"{_urlBase}/metrics/outbound/wan/{site.SchoolCode}/from/{startTimestamp}/to/{endTimestamp}"
                };

                pageList.Add(entry);
            }

            foreach (var page in pageList)
            {
                var WANInboundUsage = new List<UsagePoint>();
                var WANOutboundUsage = new List<UsagePoint>();
                var INTInboundUsage = new List<UsagePoint>();
                var INTOutboundUsage = new List<UsagePoint>();

                var WANInbound = Request(page.WANInbound);
                var WANOutbound = Request(page.WANOutbound);

                await Task.WhenAll(WANInbound, WANOutbound);

                var inboundItems = JObject.Parse(WANInbound.Result);
                IList<JToken> inboundResults = inboundItems["items"].Children().ToList();

                var outboundItems = JObject.Parse(WANOutbound.Result);
                IList<JToken> outboundResults = outboundItems["items"].Children().ToList();

                if (inboundResults.Count == 0)
                    break;

                foreach (var item in inboundResults)
                {
                    var usagePoint = item.ToObject<UsagePoint>();
                    if (WANInboundUsage.All(c => c.Time != usagePoint.Time))
                    {
                        WANInboundUsage.Add(usagePoint);
                    }
                }

                foreach (var item in outboundResults)
                {
                    var usagePoint = item.ToObject<UsagePoint>();
                    if (WANOutboundUsage.All(c => c.Time != usagePoint.Time))
                    {
                        WANOutboundUsage.Add(usagePoint);
                    }
                }

                if (page.INTInbound != null)
                {
                    var INTInbound = Request(page.INTInbound);
                    var INTOutbound = Request(page.INTOutbound);

                    await Task.WhenAll(INTInbound, INTOutbound);

                    var INTInboundItems = JObject.Parse(INTInbound.Result);
                    IList<JToken> INTInboundResults = INTInboundItems["items"].Children().ToList();

                    var INTOutboundItems = JObject.Parse(INTOutbound.Result);
                    IList<JToken> INTOutboundResults = INTOutboundItems["items"].Children().ToList();

                    if (INTInboundItems.Count == 0)
                        break;

                    foreach (var item in INTInboundResults)
                    {
                        var usagePoint = item.ToObject<UsagePoint>();
                        if (INTInboundUsage.All(c => c.Time != usagePoint.Time))
                        {
                            INTInboundUsage.Add(usagePoint);
                        }
                    }

                    foreach (var item in INTOutboundResults)
                    {
                        var usagePoint = item.ToObject<UsagePoint>();
                        if (INTOutboundUsage.All(c => c.Time != usagePoint.Time))
                        {
                            INTOutboundUsage.Add(usagePoint);
                        }
                    }
                }

                foreach (var point in WANInboundUsage)
                {
                    if (point.Time.Date != targetDate)
                        continue;

                    if (point.Time.Hour < 7)
                        continue;

                    if (point.Time.Hour > 15)
                        continue;

                    var usage = new NetworkStatisticsSiteDto.PointOfTimeUsage
                    {
                        Time = point.Time,
                        WANInbound = point.Value,
                        WANInboundDisplay = BandwidthDisplayHelper.DecimalToRoundedString(point.Value)
                    };

                    var outPoint = WANOutboundUsage.FirstOrDefault(o => o.Time == usage.Time);
                    if (outPoint == null)
                        continue;

                    usage.WANOutbound = outPoint.Value;
                    usage.WANOutboundDisplay = BandwidthDisplayHelper.DecimalToRoundedString(outPoint.Value);

                    if (page.INTInbound != null)
                    {
                        var INTinPoint = INTInboundUsage.FirstOrDefault(o => o.Time == usage.Time);
                        if (INTinPoint == null)
                            continue;

                        usage.INTInbound = INTinPoint.Value;
                        usage.INTInboundDisplay = BandwidthDisplayHelper.DecimalToRoundedString(INTinPoint.Value);

                        var INToutPoint = WANOutboundUsage.FirstOrDefault(o => o.Time == usage.Time);
                        if (INToutPoint == null)
                            continue;

                        usage.INTOutbound = INToutPoint.Value;
                        usage.INTOutboundDisplay = BandwidthDisplayHelper.DecimalToRoundedString(INToutPoint.Value);
                    }

                    site.WANData.Add(usage);
                }
            }
        }

        private NetworkStatisticsSiteDto ConvertToDto(Site site)
        {
            var data = new NetworkStatisticsSiteDto
            {
                Id = site.Id,
                RouterName = site.RouterName,
                SiteName = site.SiteName,
                SchoolCode = site.SchoolCode,
                Bandwidth = site.Bandwidth,
                WANBandwidth = site.WANBandwidth,
                INTBandwidth = site.INTBandwidth,
                BandwidthDisplay = site.BandwidthDisplay
            };

            if (site.Standards != null)
            {
                foreach (var standard in site?.Standards)
                {
                    data.Standards.Add(new NetworkStatisticsSiteDto.SiteStandard
                    {
                        Id = standard.Id,
                        Name = standard.Name
                    });
                }
            }

            if (site.WANData != null)
            {
                foreach (var item in site?.WANData)
                {
                    data.WANData.Add(new NetworkStatisticsSiteDto.PointOfTimeUsage
                    {
                        Time = item.Time,
                        INTInbound = item.INTInbound,
                        INTInboundDisplay = item.INTInboundDisplay,
                        INTOutbound = item.INTOutbound,
                        INTOutboundDisplay = item.INTOutboundDisplay,
                        WANInbound = item.WANInbound,
                        WANInboundDisplay = item.WANInboundDisplay,
                        WANOutbound = item.WANOutbound,
                        WANOutboundDisplay = item.WANOutboundDisplay
                    });
                }
            }

            return data;
        }

        private class Site
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

        private class SiteStandard
        {
            [JsonProperty("st_id")]
            public int Id { get; set; }

            [JsonProperty("st_name")]
            public string Name { get; set; }
        }

        private class PairedUsage
        {
            public DateTime Time { get; set; }
            public decimal WANInbound { get; set; }
            public string WANInboundDisplay { get; set; }
            public decimal WANOutbound { get; set; }
            public string WANOutboundDisplay { get; set; }
            public decimal INTInbound { get; set; }
            public string INTInboundDisplay { get; set; }
            public decimal INTOutbound { get; set; }
            public string INTOutboundDisplay { get; set; }
        }

        private class PairedUsagePages
        {
            public string WANInbound { get; set; }
            public string WANOutbound { get; set; }
            public string INTInbound { get; set; }
            public string INTOutbound { get; set; }
        }

        private class UsagePoint
        {
            [JsonProperty("time")]
            public DateTime Time { get; set; }

            [JsonProperty("value")]
            public decimal Value { get; set; }
        }

        private class BandwidthDetails
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
    }
}
