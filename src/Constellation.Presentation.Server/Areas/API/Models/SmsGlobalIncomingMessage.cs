namespace Constellation.Presentation.Server.Areas.API.Models;

using Application.Domains.Messaging.Sms.Models;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

internal sealed class SmsGlobalIncomingMessage
{
    // In your API project - SmsIncomingRequest.
    [FromQuery(Name = "from")]
    public string? From { get; set; }

    [FromQuery(Name = "to")]
    public string? To { get; set; }

    [FromQuery(Name = "msg")]
    public string? Msg { get; set; }

    [FromQuery(Name = "date")]
    public string? Date { get; set; }

    [FromQuery(Name = "msgid")]
    public string? MsgId { get; set; }

    public IncomingSms ToModel() => new()
    {
        From = From,
        To = To,
        Msg = Msg,
        Date = Date.AsDateTimeOffset(),
        MsgId = MsgId
    };
}
