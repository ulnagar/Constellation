namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Messaging.Sms.Models;
using Application.Interfaces.Gateways;
using Core.Shared;
using Errors;
using Microsoft.Extensions.Options;
using Model;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal sealed class Gateway : ISMSGateway
{
    private readonly HttpClient _client;

    private readonly SMSGatewayConfiguration _settings;
    private readonly ILogger _logger;
    private readonly bool _logOnly;

    public Gateway(
        HttpClient client,
        IOptions<SMSGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<ISMSGateway>();

        _settings = configuration.Value;
        _logOnly = !_settings.IsConfigured();
        _client = client;
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
            return Result.Failure<double>(SMSGatewayErrors.IncorrectResponseFromServer);

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        CreditBalance? balance = JsonSerializer.Deserialize<CreditBalance>(content);

        return balance?.Balance ?? 0f;
    }

    /// <summary>
    /// Sends a sms message.
    /// </summary>
    /// <returns>Task</returns>
    public async Task<Result<List<OutgoingSmsConfirmation>>> SendSms(
        OutgoingSms payload,
        CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("SendSms: payload={@payload}", payload);

            return new List<OutgoingSmsConfirmation>();
        }

        Guid messageId = Guid.NewGuid();
        _logger.Information("{id}: Sending SMS {@sms}", messageId, payload);

        try
        {
            HttpResponseMessage response = await RequestAsync("sms", payload, cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("{id}: Failed to send sms with error {error}", messageId, response.ReasonPhrase);

                return Result.Failure<List<OutgoingSmsConfirmation>>(SMSGatewayErrors.IncorrectResponseFromServer);
            }

            _logger.Information("{id}: Sent successfully", messageId);

            OutgoingSmsResponse? collection = await response.Content
                .ReadFromJsonAsync<OutgoingSmsResponse>(cancellationToken);

            List<OutgoingSmsConfirmation> messages = collection?.Messages ?? [];

            return messages;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "{id}: FAILED with error", messageId);

            // This is an error, so return null so the caller knows it did not complete
            return Result.Failure<List<OutgoingSmsConfirmation>>(SMSGatewayErrors.IncorrectResponseFromServer);
        }
    }

    private async Task<HttpResponseMessage> RequestAsync(
        string path,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        (string credentials, Uri uri) = Credentials(path, "GET", filter);

        using HttpRequestMessage request = new(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("MAC", credentials);

        return await _client.GetAsync(uri, cancellationToken);
    }

    private async Task<HttpResponseMessage> RequestAsync<T>(
        string path, 
        T payload, 
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        (string credentials, Uri uri) = Credentials(path, "POST", filter);

        using HttpRequestMessage request = new(HttpMethod.Post, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("MAC", credentials);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        return await _client.GetAsync(uri, cancellationToken);
    }

    private (string credentials, Uri uri) Credentials(
        string path, 
        string method = "GET", 
        string? filter = null)
    {
        string fullPath = $"https://{_settings.Host}/{_settings.Version}/{path}/";
        if (!string.IsNullOrWhiteSpace(filter))
            fullPath = $"{fullPath}?{filter}";

        Uri uri = new Uri(fullPath);

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        string mac = $"{timestamp}\n{nonce}\n{method}\n{uri.PathAndQuery}\n{uri.Host}\n{_settings.Port}\n\n";

        using HMACSHA256 hmac = new(Encoding.ASCII.GetBytes(_settings.Secret));
        mac = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(mac)));
        
        return ($"id=\"{_settings.Key}\", ts=\"{timestamp}\", nonce=\"{nonce}\", mac=\"{mac}\"", uri);
    }
}
