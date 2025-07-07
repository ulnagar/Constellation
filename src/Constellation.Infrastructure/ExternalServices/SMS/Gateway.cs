namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.DTOs;
using Application.Interfaces.Gateways;
using Core.Shared;
using Errors;
using Microsoft.Extensions.Options;
using Model;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

internal sealed class Gateway : ISMSGateway, IDisposable
{
    private Uri _uri;
    private readonly HttpClient _client;

    private readonly SMSGatewayConfiguration _settings;
    private readonly ILogger _logger;
    private readonly bool _logOnly = true;

    /// <summary>
    /// Initialize the SmsService
    /// </summary>
    /// <param name="key">smsglobal.com Account Key</param>
    /// <param name="secret">smsglobal.com Account Secret</param>
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

    /// <summary>
    /// Gets the credit balance.
    /// </summary>
    /// <returns>Task</returns>
    public async Task<Result<double>> GetCreditBalanceAsync()
    {
        if (_logOnly)
        {
            _logger.Information("GetCreditBalanceAsync");

            return 0;
        }

        HttpResponseMessage response = await RequestAsync("user/credit-balance");

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<double>(SMSGatewayErrors.IncorrectResponseFromServer);
        }

        string content = await response.Content.ReadAsStringAsync();
        CreditBalance balance = JsonConvert.DeserializeObject<CreditBalance>(content);

        return balance.balance;
    }

    /// <summary>
    /// Sends an sms message.
    /// </summary>
    /// <returns>Task</returns>
    public async Task<Result<SMSMessageCollectionDto>> SendSmsAsync(object payload)
    {
        if (_logOnly)
        {
            _logger.Information("SendSmsAsync: payload={@payload}", payload);

            return new SMSMessageCollectionDto();
        }

        Guid messageId = Guid.NewGuid();
        _logger.Information("{id}: Sending SMS {sms}", messageId, JsonConvert.SerializeObject(payload));

        try
        {
            HttpResponseMessage response = await RequestAsync("sms", payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("{id}: Failed to send sms with error {error}", messageId, response.ReasonPhrase);

                return Result.Failure<SMSMessageCollectionDto>(SMSGatewayErrors.IncorrectResponseFromServer);
            }

            _logger.Information("{id}: Sent successfully", messageId);

            string content = await response.Content.ReadAsStringAsync();
            SentMessages collection = JsonConvert.DeserializeObject<SentMessages>(content);

            return ConvertToDto(collection);
        }
        catch (Exception ex)
        {
            _logger.Warning("{id}: FAILED with error {ex}", messageId, ex.Message);

            // This is an error, so return null so the caller knows it did not complete
            return Result.Failure<SMSMessageCollectionDto>(SMSGatewayErrors.IncorrectResponseFromServer);
        }
    }

    /// <summary>
    /// Requests information from the rest api.
    /// </summary>
    /// <param name="path">The rest api path.</param>
    /// <param name="payload">The rest api method.</param>
    /// <param name="filter">The rest api query string result filter.</param>
    /// <returns>The http response message object.</returns>
    private async Task<HttpResponseMessage> RequestAsync(string path, object payload = null, string filter = "")
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
                    response = await _client.GetAsync(_uri);
                }
                else
                {
                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    response = await _client.PostAsync(_uri, content);
                    content.Dispose();
                }

                return response;
            }
            catch
            {
                // Wait and retry
                await Task.Delay(5000);
            }
        }

        return new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
    }

    /// <summary>
    /// Compiles the mac oauth2 credentials.
    /// </summary>
    /// <param name="path">The request path.</param>
    /// <param name="method">The request method.</param>
    /// <returns>The credential string.</returns>
    private string Credentials(string path, string method = "GET", string filter = "")
    {
        var fullPath = $"https://{_settings.Host}/{_settings.Version}/{path}/";
        if (!string.IsNullOrWhiteSpace(filter))
            fullPath = $"{fullPath}?{filter}";

        _uri = new Uri(fullPath);

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var mac = $"{timestamp}\n{nonce}\n{method}\n{_uri.PathAndQuery}\n{_uri.Host}\n{_settings.Port}\n\n";

        var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_settings.Secret));
        mac = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(mac)));
        hmac.Dispose();

        return $"id=\"{_settings.Key}\", ts=\"{timestamp}\", nonce=\"{nonce}\", mac=\"{mac}\"";
    }

    private static SMSMessageCollectionDto ConvertToDto(SentMessages collection)
    {
        var data = new SMSMessageCollectionDto();

        foreach (var message in collection.messages)
        {
            data.Messages.Add(new SMSMessageCollectionDto.Message
            {
                Id = message.id,
                Destination = message.destination,
                MessageBody = message.message,
                Origin = message.origin,
                OutgoingId = message.outgoing_id,
                Status = message.status,
                Timestamp = message.dateTime
            });
        }

        return data;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
