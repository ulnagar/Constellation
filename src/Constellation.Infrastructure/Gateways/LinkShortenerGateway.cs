using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Gateways
{
    public class LinkShortenerGateway : ILinkShortenerGateway, IScopedService
    {
        private readonly HttpClient _client;
        private readonly ILinkShortenerGatewayConfiguration _settings;
        private readonly ILogger<ILinkShortenerGateway> _logger;

        public LinkShortenerGateway(ILinkShortenerGatewayConfiguration settings, ILogger<ILinkShortenerGateway> logger)
        {
            _settings = settings;
            _logger = logger;
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
            _logger.LogInformation("{id}: Attempting to shorten url : {url}", requestId, url);

            var linkInfo = new DynamicLink();
            linkInfo.dynamicLinkInfo.domainUriPrefix = "https://aurora.link";
            linkInfo.dynamicLinkInfo.link = url;
            linkInfo.suffix.option = DynamicLinkSuffix.Short;

            _logger.LogInformation("{id}: Using object : {object}", requestId, JsonConvert.SerializeObject(linkInfo));
#if DEBUG
            return url;
#else
            var response = await _client.PostAsJsonAsync($"{_settings.ApiEndpoint}?key={_settings.ApiKey}", linkInfo);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var link = JsonConvert.DeserializeObject<ShortLinkInfo>(content).ShortLink;
                
                _logger.LogInformation("{id}: Successfully shortened url to {url}", requestId, link);

                return link;
            } else
            {
                _logger.LogWarning("{id}: Failed to shorten url : {url}. Failed with error {error}", requestId, url, response.ReasonPhrase);
                response.EnsureSuccessStatusCode();
                return null;
            }
#endif
        }

        private class ShortLinkInfo
        {
            public string ShortLink { get; set; }
            public string PreviewLink { get; set; }
        }

        private class DynamicLink
        {
            public DynamicLinkInfo dynamicLinkInfo { get; set; }

            public DynamicLinkSuffix suffix { get; set; }

            public DynamicLink()
            {
                dynamicLinkInfo = new DynamicLinkInfo();
                suffix = new DynamicLinkSuffix();
            }
        }

        private class DynamicLinkInfo
        {
            public string domainUriPrefix { get; set; }
            public string link { get; set; }
        }

        private class DynamicLinkSuffix
        {
            public const string Short = "SHORT";
            public const string Long = "UNGUESSABLE";

            public string option { get; set; }
        }
    }
}
