namespace Constellation.Application.Domains.Messaging.Sms.Dtos;

using Helpers.JsonConverters;
using System.Text.Json.Serialization;

public sealed class IncomingSmsEntry
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? Id { get; set; }

    [JsonPropertyName("outgoing_id")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? OutgoingId { get; set; }

    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    [JsonPropertyName("destination")]
    public string? Destination { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("isUnicode")]
    public bool? IsUnicode { get; set; }

    [JsonPropertyName("dateTime")]
    [JsonConverter(typeof(FlexibleDateTimeOffsetConverter))]
    public DateTimeOffset DateTime { get; set; }

    [JsonPropertyName("isMultipart")]
    public bool IsMultipart { get; set; }

    [JsonPropertyName("partNumber")]
    public int? PartNumber { get; set; }

    [JsonPropertyName("totalParts")]
    public int? TotalParts { get; set; }
}