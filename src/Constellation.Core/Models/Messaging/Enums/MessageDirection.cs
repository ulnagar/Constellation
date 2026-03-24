namespace Constellation.Core.Models.Messaging.Enums;

using Common;

public sealed class MessageDirection : StringEnumeration<MessageDirection>
{
    public static readonly MessageDirection Outbound = new("Outbound");
    public static readonly MessageDirection Inbound = new("Inbound");
    private MessageDirection(string value) 
        : base(value, value) { }
    public IEnumerable<MessageDirection> GetOptions => GetEnumerable;
}
