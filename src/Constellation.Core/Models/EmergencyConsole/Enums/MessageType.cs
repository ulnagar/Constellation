namespace Constellation.Core.Models.EmergencyConsole.Enums;

using Common;
using System.Collections.Generic;

public sealed class MessageType : StringEnumeration<MessageType>
{
    public static readonly MessageType Email = new("Email", "Email");
    public static readonly MessageType SMS = new("SMS", "SMS");

    private MessageType(string value, string name) 
        : base(value, name)
    {
    }

    public static IEnumerable<MessageType> GetOptions => GetEnumerable;
}