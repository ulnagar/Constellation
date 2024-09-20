namespace Constellation.Infrastructure.ExternalServices.DoEDataSourcesGateway;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

internal class Gateway : IDoEDataSourcesGateway
{
    private readonly HttpClient _client;
    private readonly DoEDataSourcesGatewayConfiguration _configuration;

    private readonly bool _logOnly = true;
    private readonly ILogger _logger;

    public Gateway(
        IOptions<DoEDataSourcesGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<IDoEDataSourcesGateway>();

        _configuration = configuration.Value;

        _logOnly = !_configuration.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initalised in log only mode");

            return;
        }

        HttpClientHandler config = new HttpClientHandler
        {
            CookieContainer = new CookieContainer()
        };

        IWebProxy proxy = WebRequest.DefaultWebProxy;
        config.UseProxy = true;
        config.Proxy = proxy;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        _client = new HttpClient(config);
    }

    public async Task<List<CeseSchoolResponse>> GetSchoolsFromCESEMasterData()
    {
        if (_logOnly)
        {
            _logger.Information("GetSchoolsFromMasterData");

            return new List<CeseSchoolResponse>();
        }

        HttpResponseMessage response = await _client.GetAsync(_configuration.CESEDataPath);

        string json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
            return new List<CeseSchoolResponse>();

        try
        {
            List<CeseSchoolResponse> results = JsonSerializer.Deserialize<List<CeseSchoolResponse>>(json);

            List<CeseSchoolResponse> newResults = results.Where(entry =>
                    entry.GetType().GetProperties()
                        .Where(pi => pi.PropertyType == typeof(string))
                        .Select(pi => (string)pi.GetValue(entry))
                        .Any(value => !string.IsNullOrEmpty(value)))
                .ToList();

            return newResults;
        }
        catch (Exception ex)
        {
            _logger.Warning("GetSchoolsFromCESEMasterData: Failed to deserialize data with error {@ex}", ex);

            return new List<CeseSchoolResponse>();
        }
    }

    public async Task<List<DataCollectionsSchoolResponse>> GetSchoolsFromDataCollections()
    {
        if (_logOnly)
        {
            _logger.Information("GetSchoolsFromDataCollections");

            return new List<DataCollectionsSchoolResponse>();
        }

        HttpResponseMessage response = await _client.GetAsync(_configuration.DataCollectionsDataPath);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        List<string> entries = content.Split('\u000A').ToList();

        TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;

        List<DataCollectionsSchoolResponse> list = new List<DataCollectionsSchoolResponse>();
        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        foreach (string entry in entries)
        {
            string[] splitString = CSVParser.Split(entry);
            if (splitString[0].Length > 4 || splitString.Length == 1)
                continue;

            try
            {
                DataCollectionsSchoolResponse school = new DataCollectionsSchoolResponse
                {
                    SchoolCode = splitString[0].Trim(),
                    Name = splitString[9].Trim('"').Trim(),
                    Address = splitString[10].Trim(),
                    Town = textInfo.ToTitleCase(splitString[11].Trim()),
                    PostCode = splitString[12].Trim(),
                    Status = splitString[16].Trim(),
                    Electorate = splitString[19].Trim(),
                    PrincipalNetwork = splitString[29].Trim(),
                    Division = splitString[28].Trim(), // School Performance Directorate
                    PhoneNumber = Regex.Replace(splitString[47].Trim(), @"[^0-9]", ""),
                    EmailAddress = splitString[49].Trim(),
                    FaxNumber = Regex.Replace(splitString[50].Trim(), @"[^0-9]", ""),
                    HeatSchool = splitString[54] == "Y",
                    PrincipalName = splitString[75].Trim(),
                    PrincipalEmail = splitString[76].Trim()
                };

                if (school.PhoneNumber.Length == 8)
                    school.PhoneNumber = $"02{school.PhoneNumber}";

                if (school.FaxNumber.Length == 8)
                    school.FaxNumber = $"02{school.FaxNumber}";

                if (school.PrincipalName.IndexOf(',') > 0)
                {
                    school.PrincipalName = school.PrincipalName.Trim('"').Trim();
                    string[] principalName = school.PrincipalName.Split(',');
                    school.PrincipalName = $"{principalName[1].Trim()} {principalName[0].Trim()}";
                    school.PrincipalFirstName = principalName[1].Trim();
                    school.PrincipalLastName = principalName[0].Trim();
                }

                list.Add(school);
            }
            catch (Exception)
            {

                throw;
            }
        }

        return list;
    }
}
