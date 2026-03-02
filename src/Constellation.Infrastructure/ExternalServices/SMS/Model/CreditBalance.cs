namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

using System.Text.Json.Serialization;

internal sealed class CreditBalance
{
    [JsonPropertyName("balance")]
    public double Balance { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}
