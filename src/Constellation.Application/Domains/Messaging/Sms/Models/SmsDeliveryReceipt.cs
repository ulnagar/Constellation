namespace Constellation.Application.Domains.Messaging.Sms.Models;

using System.Text.Json.Serialization;

public sealed class SmsDeliveryReceipt
{
    public string? Id { get; set; }
    [JsonPropertyName("outgoing_id")]
    public string? OutgoingId { get; set; }
    public string? Status { get; set; }
    [JsonPropertyName("update_time")]
    public DateTimeOffset UpdateTime { get; set; }
}
