namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Messaging.Sms.Models;
using Application.Interfaces.Gateways;
using Core.Shared;
using Errors;
using Microsoft.Extensions.Options;
using Model;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal sealed class Gateway : ISMSGateway, IDisposable
{
    private Uri _uri;
    private readonly HttpClient _client;

    private readonly SMSGatewayConfiguration _settings;
    private readonly ILogger _logger;
    private readonly bool _logOnly;

    public Gateway(
        IOptions<SMSGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<ISMSGateway>();

        _settings = configuration.Value;
        _logOnly = !_settings.IsConfigured();

        if (_logOnly)
        {
            return;
        }

        HttpClientHandler config = new()
        {
            CookieContainer = new CookieContainer()
        };

        IWebProxy? proxy = WebRequest.DefaultWebProxy;
        config.UseProxy = true;
        config.Proxy = proxy;

        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        _client = new HttpClient(config);
        config.Dispose();
    }

    /// <summary>
    /// Gets the credit balance.
    /// </summary>
    /// <returns>Task</returns>
    public async Task<Result<double>> GetCreditBalance(
        CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetCreditBalance");

            return 0;
        }

        HttpResponseMessage response = await RequestAsync("user/credit-balance", cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<double>(SMSGatewayErrors.IncorrectResponseFromServer);
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        CreditBalance? balance = JsonSerializer.Deserialize<CreditBalance>(content);

        return balance?.Balance ?? 0f;
    }

    /// <summary>
    /// Sends a sms message.
    /// </summary>
    /// <returns>Task</returns>
    public async Task<Result<List<OutgoingSmsConfirmation>>> SendSms(
        object payload,
        CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("SendSms: payload={@payload}", payload);

            return new List<OutgoingSmsConfirmation>();
        }

        Guid messageId = Guid.NewGuid();
        _logger.Information("{id}: Sending SMS {sms}", messageId, JsonSerializer.Serialize(payload));

        try
        {
            HttpResponseMessage response = await RequestAsync("sms", payload, cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("{id}: Failed to send sms with error {error}", messageId, response.ReasonPhrase);

                return Result.Failure<List<OutgoingSmsConfirmation>>(SMSGatewayErrors.IncorrectResponseFromServer);
            }

            _logger.Information("{id}: Sent successfully", messageId);

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            List<OutgoingSmsConfirmation>? collection = JsonSerializer.Deserialize<List<OutgoingSmsConfirmation>>(content);

            return collection;
        }
        catch (Exception ex)
        {
            _logger.Warning("{id}: FAILED with error {ex}", messageId, ex.Message);

            // This is an error, so return null so the caller knows it did not complete
            return Result.Failure<List<OutgoingSmsConfirmation>>(SMSGatewayErrors.IncorrectResponseFromServer);
        }
    }

    private async Task<HttpResponseMessage> RequestAsync(
        string path, 
        object? payload = null, 
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        string credentials = Credentials(path, null == payload ? "GET" : "POST", filter);

        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("MAC", credentials);
        
        for (int i = 1; i < 6; i++)
        {
            try
            {
                HttpResponseMessage response;

                if (payload == null)
                {
                    response = await _client.GetAsync(_uri, cancellationToken);
                }
                else
                {
                    string jsonPayload = JsonSerializer.Serialize(payload);
                    StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");

                    response = await _client.PostAsync(_uri, content, cancellationToken);
                    content.Dispose();
                }

                return response;
            }
            catch
            {
                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
    }

    private string Credentials(
        string path, 
        string method = "GET", 
        string? filter = null)
    {
        string fullPath = $"https://{_settings.Host}/{_settings.Version}/{path}/";
        if (!string.IsNullOrWhiteSpace(filter))
            fullPath = $"{fullPath}?{filter}";

        _uri = new Uri(fullPath);

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        string mac = $"{timestamp}\n{nonce}\n{method}\n{_uri.PathAndQuery}\n{_uri.Host}\n{_settings.Port}\n\n";

        HMACSHA256 hmac = new(Encoding.ASCII.GetBytes(_settings.Secret));
        mac = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(mac)));
        hmac.Dispose();

        return $"id=\"{_settings.Key}\", ts=\"{timestamp}\", nonce=\"{nonce}\", mac=\"{mac}\"";
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
