namespace Constellation.Core.Models.EmergencyConsole.Enums;

using Common;

public sealed class MessageStatus : StringEnumeration<MessageStatus>
{
    public static readonly MessageStatus Pending = new("Pending", "Pending");
    public static readonly MessageStatus Error = new("Error", "Error");
    public static readonly MessageStatus Sent = new("Sent", "Sent");
    public static readonly MessageStatus Delivered = new("Delivered", "Delivered");
    
    private MessageStatus(string value, string name)
        : base(value, name)
    {
    }

    public static IEnumerable<MessageStatus> GetOptions => GetEnumerable;
}