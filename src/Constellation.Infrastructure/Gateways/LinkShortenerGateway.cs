using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using Newtonsoft.Json;
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

        public LinkShortenerGateway(ILinkShortenerGatewayConfiguration settings)
        {
            _settings = settings;

            var config = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            var proxy = WebRequest.DefaultWebProxy;
            config.UseProxy = true;
            config.Proxy = proxy;

            _client = new HttpClient(config);
        }

        public async Task<string> ShortenURL(string url)
        {
            var linkInfo = new DynamicLink();
            linkInfo.dynamicLinkInfo.domainUriPrefix = "https://aurora.link";
            linkInfo.dynamicLinkInfo.link = url;
            linkInfo.suffix.option = DynamicLinkSuffix.Short;

            var response = await _client.PostAsJsonAsync($"{_settings.ApiEndpoint}?key={_settings.ApiKey}", linkInfo);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShortLinkInfo>(content).ShortLink;
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
