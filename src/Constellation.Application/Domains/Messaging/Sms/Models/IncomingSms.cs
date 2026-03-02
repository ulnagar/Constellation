namespace Constellation.Application.Domains.Messaging.Sms.Models;

public sealed class IncomingSms
{
    public string? From { get; set; }
    public long To { get; set; }
    public string? Msg { get; set; }
    public string? Date { get; set; }
    public long MsgId { get; set; }
}
