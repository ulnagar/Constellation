namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics;

using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.NetworkStatistics.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net.Http.Headers;


internal sealed class Gateway : INetworkStatisticsGateway
{
    private readonly HttpClient _client;
    private string _urlBase;

    private readonly bool _logOnly = true;
    private readonly ILogger _logger;

    public Gateway(
        IOptions<NetworkStatisticsGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<INetworkStatisticsGateway>();

        _logOnly = !configuration.Value.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initalised in log only mode");

            return;
        }

        _urlBase = configuration.Value.Url;

        _client = new HttpClient();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.Value.Key);
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

        if (_logOnly)
        {
            _logger.Information("GetSiteDetails: schoolCode={schoolCode}, queryUri={queryUri}", schoolCode, queryUri);

            return ConvertToDto(new Site()
            {
                Id = 1,
                RouterName = "first-p",
                SiteName = "First Public School",
                SchoolCode = "1111",
                Bandwidth = 100,
                WANBandwidth = 100,
                INTBandwidth = 100,
                BandwidthDisplay = "100Mbps"
            });
        }

        var details = await Request(queryUri);
        var siteObject = JObject.Parse(details);
        IList<JToken> results = siteObject["items"].Children().ToList();

        IList<Site> sites = new List<Site>();
        foreach (var site in results)
        {
            var budSite = site.ToObject<Site>();
            budSite.BandwidthDisplay = Convert.ToDecimal(budSite.Bandwidth).ToFormattedString();
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

        if (_logOnly)
        {
            _logger.Information("GetSiteUsage: site={@site}, day={day}, queryUri={queryUri}", site, day, queryUri);

            return;
        }

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
                    WANInbound = point.Value ?? 0,
                    WANInboundDisplay = point.Value.ToRoundedString()
                };

                var outPoint = WANOutboundUsage.FirstOrDefault(o => o.Time == usage.Time);
                if (outPoint == null)
                    continue;

                usage.WANOutbound = outPoint.Value ?? 0;
                usage.WANOutboundDisplay = outPoint.Value.ToRoundedString();

                if (page.INTInbound != null)
                {
                    var INTinPoint = INTInboundUsage.FirstOrDefault(o => o.Time == usage.Time);
                    if (INTinPoint == null)
                        continue;

                    usage.INTInbound = INTinPoint.Value ?? 0;
                    usage.INTInboundDisplay = INTinPoint.Value.ToRoundedString();

                    var INToutPoint = WANOutboundUsage.FirstOrDefault(o => o.Time == usage.Time);
                    if (INToutPoint == null)
                        continue;

                    usage.INTOutbound = INToutPoint.Value ?? 0;
                    usage.INTOutboundDisplay = INToutPoint.Value.ToRoundedString();
                }

                site.WANData.Add(usage);
            }
        }
    }

    private static NetworkStatisticsSiteDto ConvertToDto(Site site)
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
}
