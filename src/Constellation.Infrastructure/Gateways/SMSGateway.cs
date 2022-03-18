using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Gateways
{
    public class SMSGateway : ISMSGateway, IScopedService
    {
        private Uri _uri;
        private readonly HttpClient _client;

        private readonly string _host;
        private readonly string _port;
        private readonly string _version;
        private readonly string _key;
        private readonly string _secret;
        private readonly ILogger<ISMSGateway> _logger;

        /// <summary>
        /// Initialize the SmsService
        /// </summary>
        /// <param name="key">smsglobal.com Account Key</param>
        /// <param name="secret">smsglobal.com Account Secret</param>
        public SMSGateway(ISMSGatewayConfiguration configuration, ILogger<ISMSGateway> logger)
        {
            _host = "api.smsglobal.com";
            _port = "443";
            _version = "v2";
            _key = configuration.Key;
            _secret = configuration.Secret;

            var config = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            var proxy = WebRequest.DefaultWebProxy;
            config.UseProxy = true;
            config.Proxy = proxy;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            _client = new HttpClient(config);
            _logger = logger;
        }

        /// <summary>
        /// Gets the credit balance.
        /// </summary>
        /// <returns>Task</returns>
        public async Task<double> GetCreditBalanceAsync()
        {
            HttpResponseMessage response = await RequestAsync("user/credit-balance");

            var content = await response.Content.ReadAsStringAsync();
            var balance = JsonConvert.DeserializeObject<CreditBalance>(content);

            return balance.balance;
        }

        /// <summary>
        /// Sends an sms message.
        /// </summary>
        /// <returns>Task</returns>
        public async Task<SMSMessageCollectionDto> SendSmsAsync(Object payload)
        {
            var messageId = Guid.NewGuid();
            _logger.LogInformation("{id}: Sending SMS {sms}", messageId, payload);

            try
            {
                HttpResponseMessage response = await RequestAsync("sms", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("{id}: Failed to send sms with error {error}", messageId, response.ReasonPhrase);
                } else
                {
                    _logger.LogInformation("{id}: Sent successfully", messageId);
                }

                var content = await response.Content.ReadAsStringAsync();
                var collection = JsonConvert.DeserializeObject<SentMessages>(content);

                return ConvertToDto(collection);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("{id}: FAILED with error {ex}", messageId, ex.Message);

                var data = new SentMessages
                {
                    messages = Array.Empty<SentMessage>()
                };

                return ConvertToDto(data);
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

            HttpResponseMessage response;

            for (int i = 1; i < 6; i++)
            {
                try
                {
                    if (payload == null)
                    {
                        response = await _client.GetAsync(_uri.ToString());
                    }
                    else
                    {
                        response = await _client.PostAsJsonAsync(_uri.ToString(), (SMSMessageToSend)payload);
                    }

                    return response;
                }
                catch
                {
                    // Wait and retry
                    await Task.Delay(5000);
                }
            }

            throw new Exception($"Could not connect to SMS Gateway");
        }

        /// <summary>
        /// Compiles the mac oauth2 credentials.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="method">The request method.</param>
        /// <returns>The credential string.</returns>
        private string Credentials(string path, string method = "GET", string filter = "")
        {
            if (!string.IsNullOrWhiteSpace(filter))
                _uri = new Uri($"https://{_host}/{_version}/{path}/?{filter}");
            else
                _uri = new Uri($"https://{_host}/{_version}/{path}/");

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var mac = $"{timestamp}\n{nonce}\n{method}\n{_uri.PathAndQuery}\n{_uri.Host}\n{_port}\n\n";

            mac = Convert.ToBase64String((new HMACSHA256(Encoding.ASCII.GetBytes(_secret))).ComputeHash(Encoding.ASCII.GetBytes(mac)));

            return $"id=\"{_key}\", ts=\"{timestamp}\", nonce=\"{nonce}\", mac=\"{mac}\"";
        }

        private SMSMessageCollectionDto ConvertToDto(SentMessages collection)
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

        private class Response
        {
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        private class SentMessage
        {
            public string id { get; set; }
            public string outgoing_id { get; set; }
            public string origin { get; set; }
            public string message { get; set; }
            public string dateTime { get; set; }
            public string status { get; set; }
            public string destination { get; set; }
        }

        private class SentMessages : Response
        {
            public SentMessage[] messages { get; set; }
        }

        private class CreditBalance : Response
        {
            public double balance { get; set; }
            public string currency { get; set; }
        }
    }
}
