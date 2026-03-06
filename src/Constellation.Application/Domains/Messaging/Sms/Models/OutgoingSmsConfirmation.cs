namespace Constellation.Application.Domains.Messaging.Sms.Models;

using Helpers.JsonConverters;
using System.Text.Json.Serialization;

public sealed class OutgoingSmsConfirmation
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? Id { get; set; }                  // Message part identifier

    [JsonPropertyName("outgoing_id")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? OutgoingId { get; set; }          // Outgoing message identifier

    [JsonPropertyName("origin")]
    public string? Origin { get; set; }              // The from/sender number or ID

    [JsonPropertyName("destination")]
    public string? Destination { get; set; }         // The to/recipient number

    [JsonPropertyName("message")]
    public string? Message { get; set; }             // The message content

    [JsonPropertyName("dateTime")]
    [JsonConverter(typeof(FlexibleDateTimeOffsetConverter))]
    public DateTimeOffset DateTime { get; set; }     // When SMSGlobal accepted the message

    [JsonPropertyName("status")]
    public string? Status { get; set; }              // e.g. "sent", "failed"
}