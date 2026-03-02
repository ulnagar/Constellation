namespace Constellation.Application.Domains.Messaging.Sms.Models;

public sealed class SmsDeliveryReceipt
{
    public long Id { get; set; }
    public long OutgoingId { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
}
