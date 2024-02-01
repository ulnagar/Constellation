namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System.Text.Json.Serialization;

public sealed class LissCall
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonPropertyName("params")]
    public object[] Params { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }
}
