namespace Constellation.Application.Domains.Messaging.Sms.Dtos;

public sealed class IncomingSms
{
    public string? From { get; set; }

    public string? To { get; set; }

    public string? Msg { get; set; }

    public DateTimeOffset Date { get; set; }

    public string? MsgId { get; set; }
}