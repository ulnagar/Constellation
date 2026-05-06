namespace Constellation.Application.Domains.Messaging.Sms.Dtos;

using Constellation.Application.Helpers.JsonConverters;
using System.Text.Json.Serialization;

public sealed class SmsDeliveryReceipt
{
    [JsonPropertyName("outgoingId")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? OutgoingId { get; set; }

    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    [JsonPropertyName("destination")]
    public string? Destination { get; set; }

    [JsonPropertyName("dateTime")]
    [JsonConverter(typeof(FlexibleDateTimeOffsetConverter))]
    public DateTimeOffset DateTime { get; set; }

    [JsonPropertyName("message_ids")]
    public List<string> MessageIds { get; set; } = [];

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
