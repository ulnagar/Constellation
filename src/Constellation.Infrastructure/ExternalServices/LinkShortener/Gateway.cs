namespace Constellation.Infrastructure.ExternalServices.LinkShortener;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.LinkShortener.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

internal class Gateway : ILinkShortenerGateway
{
    private readonly HttpClient _client;
    private readonly LinkShortenerGatewayConfiguration _settings;
    private readonly ILogger _logger;

    private readonly bool _logOnly = true;

    public Gateway(
        IOptions<LinkShortenerGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<ILinkShortenerGateway>();
        
        _settings = configuration.Value;
        _logOnly = !_settings.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initalised in log only mode");

            return;
        }

        var config = new HttpClientHandler
        {
            CookieContainer = new CookieContainer()
        };

        var proxy = WebRequest.DefaultWebProxy;
        config.UseProxy = true;
        config.Proxy = proxy;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        _client = new HttpClient(config);
    }

    public async Task<string> ShortenURL(string url)
    {
        var requestId = Guid.NewGuid();
        _logger.Information("{id}: Attempting to shorten url : {url}", requestId, url);

        var linkInfo = new DynamicLink();
        linkInfo.dynamicLinkInfo.domainUriPrefix = "https://aurora.link";
        linkInfo.dynamicLinkInfo.link = url;
        linkInfo.suffix.option = DynamicLinkSuffix.Short;

        _logger.Information("{id}: Using object : {object}", requestId, JsonConvert.SerializeObject(linkInfo));

        if (_logOnly)
        {
            _logger.Information("ShortenURL: Log Only Mode");

            return url;
        }

        var response = await _client.PostAsJsonAsync($"{_settings.ApiEndpoint}?key={_settings.ApiKey}", linkInfo);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var link = JsonConvert.DeserializeObject<ShortLinkInfo>(content).ShortLink;
            
            _logger.Information("{id}: Successfully shortened url to {url}", requestId, link);

            return link;
        } else
        {
            _logger.Warning("{id}: Failed to shorten url : {url}. Failed with error {error}", requestId, url, response.ReasonPhrase);
            response.EnsureSuccessStatusCode();
            return null;
        }
    }
}
