namespace Constellation.Application.Domains.Messaging.Sms.Models;

public sealed class SmsDeliveryReceipt
{
    public string? Id { get; set; }
    public string? OutgoingId { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
}
