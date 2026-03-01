namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Constellation.Application.Domains.Messaging.Sms.Enums;
using Constellation.Application.Domains.Messaging.Sms.Models;
using Models;
using Serilog;
using System.Globalization;

public static class SmsEndpoints
{
    private static readonly Serilog.ILogger _logger = Log.Logger.ForContext(typeof(SmsEndpoints));

    public static void MapSmsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sms/incoming", HandleIncomingSms)
            .WithName("SmsIncoming")
            .Accepts<IncomingSms>("application/json");

        app.MapPost("/api/sms/delivery-receipt", HandleDeliveryReceipt)
            .WithName("SmsDeliveryReceipt")
            .Accepts<SmsDeliveryReceipt>("application/json");
    }

    private static IResult HandleIncomingSms(IncomingSms message)
    {
        if (string.IsNullOrWhiteSpace(message.From) || string.IsNullOrWhiteSpace(message.Msg))
        {
            _logger.Warning("Received malformed SMSGlobal postback");
            return Results.BadRequest();
        }

        _logger.Information(
            "Incoming SMS | From: {From} | To: {To} | MsgId: {MsgId} | Date: {Date} | Message: {Msg}",
            message.From, message.To, message.MsgId, message.Date, message.Msg);

        var inboundMessage = new SmsMessage
        {
            SmsGlobalId = message.MsgId,
            From = message.From,
            To = message.To.ToString(CultureInfo.InvariantCulture),
            Message = message.Msg!,
            Direction = SmsDirection.Inbound,
            Status = SmsStatus.Received,
            CreatedAt = DateTimeOffset.UtcNow,
            SmsGlobalDate = DateTimeOffset.Parse(message.Date!, DateTimeFormatInfo.CurrentInfo),
            ReplyToId = originalMessage?.Id   // null if no match found
        };

        // SMSGlobal requires the response body to contain "OK"
        return Results.Ok("OK");
    }

    private static IResult HandleDeliveryReceipt(SmsDeliveryReceipt receipt)
    {
        if (receipt.Id == 0 || string.IsNullOrWhiteSpace(receipt.Status))
        {
            _logger.Warning("Received malformed SMSGlobal delivery receipt");
            return Results.BadRequest();
        }

        _logger.Information(
            "Delivery Receipt | Id: {Id} | OutgoingId: {OutgoingId} | Status: {Status} | UpdateTime: {UpdateTime}",
            receipt.Id, receipt.OutgoingId, receipt.Status, receipt.UpdateTime);

        var existing = await _db.SmsMessages
            .FirstOrDefaultAsync(m => m.OutgoingId == receipt.OutgoingId);

        if (existing is not null)
        {
            existing.Status = receipt.Status switch
            {
                "Delivered" => SmsStatus.Delivered,
                "Failed" => SmsStatus.Failed,
                _ => existing.Status
            };
            existing.StatusUpdatedAt = receipt.UpdateTime;
            await _db.SaveChangesAsync();
        }

        return Results.Ok("OK");
    }
}
