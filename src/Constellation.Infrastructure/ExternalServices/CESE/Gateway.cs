using Constellation.Application.DTOs.Json;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Constellation.Infrastructure.ExternalServices.CESE
{
    public class Gateway : ICeseGateway, IScopedService
    {
        private readonly HttpClient _client;

        public Gateway()

        {
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

        public async Task<IList<JsonSchoolDto>> GetSchoolsFromMasterData()
        {
            var response = await _client.GetAsync($"https://data.cese.nsw.gov.au/data/dataset/027493b2-33ad-3f5b-8ed9-37cdca2b8650/resource/af20d17c-a7ac-4251-af75-e5ae66573e92/download/masterdatasetnightlybatchcollections.json");

            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
                return new List<JsonSchoolDto>();

            try
            {
                var results = JsonSerializer.Deserialize<IList<JsonSchoolDto>>(json);

                var newResults = results.Where(entry =>
                        entry.GetType().GetProperties()
                            .Where(pi => pi.PropertyType == typeof(string))
                            .Select(pi => (string)pi.GetValue(entry))
                            .Any(value => !string.IsNullOrEmpty(value)))
                    .ToList();

                return newResults;
            }
            catch
            {
                return new List<JsonSchoolDto>();
            }
        }
    }
}
