namespace Constellation.Application.Domains.Messaging.Sms.Models;

using Helpers.JsonConverters;
using System.Text.Json.Serialization;

public sealed class IncomingSms
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? To { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("date")]
    [JsonConverter(typeof(FlexibleDateTimeOffsetConverter))]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("msgid")]
    [JsonConverter(typeof(NumberOrStringConverter))]
    public string? MsgId { get; set; }
}