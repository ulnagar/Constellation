namespace Constellation.Application.Domains.Messaging.Sms.Models;

using Constellation.Application.Helpers.JsonConverters;
using System.Text.Json.Serialization;

public sealed class SmsDeliveryReceipt
{
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? Id { get; set; }
    [JsonPropertyName("outgoing_id")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? OutgoingId { get; set; }
    public string? Status { get; set; }
    [JsonPropertyName("update_time")]
    [JsonConverter(typeof(FlexibleDateTimeOffsetConverter))]
    public DateTimeOffset UpdateTime { get; set; }
}
