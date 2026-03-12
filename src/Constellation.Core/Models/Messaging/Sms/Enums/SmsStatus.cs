namespace Constellation.Core.Models.Messaging.Sms.Enums;

using Common;

public sealed class SmsStatus : StringEnumeration<SmsStatus>
{
    public static readonly SmsStatus Pending = new("Pending");
    public static readonly SmsStatus Sent = new("Sent");
    public static readonly SmsStatus Delivered = new("Delivered");
    public static readonly SmsStatus Failed = new("Failed");
    public static readonly SmsStatus Received = new("Received");
    
    private SmsStatus(string value) 
        : base(value, value) { }

    public IEnumerable<SmsStatus> GetOptions => GetEnumerable;
}
