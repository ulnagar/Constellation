namespace Constellation.Infrastructure.ExternalServices.CESE;

using Constellation.Application.DTOs.Json;
using Constellation.Application.Interfaces.Gateways;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

internal class Gateway : ICeseGateway
{
    private readonly HttpClient _client;
    private readonly CESEGatewayConfiguration _configuration;

    private readonly bool _logOnly = true;
    private readonly ILogger _logger;

    public Gateway(
        IOptions<CESEGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<ICeseGateway>();

        _configuration = configuration.Value;

        _logOnly = !_configuration.IsConfigured();

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

    public async Task<IList<JsonSchoolDto>> GetSchoolsFromMasterData()
    {
        if (_logOnly)
        {
            _logger.Information("GetSchoolsFromMasterData");

            return new List<JsonSchoolDto>();
        }

        var response = await _client.GetAsync(_configuration.DataPath);

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
        catch (Exception ex)
        {
            _logger.Warning("GetSchoolsFromMasterData: Failed to deserialize data with error {@ex}", ex);

            return new List<JsonSchoolDto>();
        }
    }
}
