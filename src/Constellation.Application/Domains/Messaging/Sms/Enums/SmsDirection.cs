namespace Constellation.Application.Domains.Messaging.Sms.Enums;

using Core.Common;
using System.Collections.Generic;

public sealed class SmsDirection : StringEnumeration<SmsDirection>
{
    public static readonly SmsDirection Outbound = new("Outbound");
    public static readonly SmsDirection Inbound = new("Inbound");
    private SmsDirection(string value) 
        : base(value, value) { }
    public IEnumerable<SmsDirection> GetOptions => GetEnumerable;
}
