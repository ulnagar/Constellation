namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;
using Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;
using Constellation.Application.Domains.Messaging.Sms.Models;
using MediatR;
using Serilog;
using System.Text;
using System.Text.Json;

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

    private static async Task<IResult> HandleIncomingSms(HttpContext context, ISender mediator)
    {
        context.Request.EnableBuffering();
        using StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        string rawBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger
            .ForContext("rawBody", rawBody)
            .Information("Received Incoming SMS");

        IncomingSms? message = JsonSerializer.Deserialize<IncomingSms>(rawBody);

        if (message is null || string.IsNullOrWhiteSpace(message.From) || string.IsNullOrWhiteSpace(message.Msg))
        {
            _logger
                .ForContext(nameof(IncomingSms), message, true)
                .Warning("Received malformed Incoming SMS");

            return Results.BadRequest();
        }

        _logger
            .ForContext(nameof(IncomingSms), message, true)
            .Information("Incoming SMS From: {From}", message.From);

        await mediator.Send(new CreateNewIncomingSmsRecordCommand(message));

        return Results.Ok("OK");
    }

    private static async Task<IResult> HandleDeliveryReceipt(HttpContext context, ISender mediator)
    {
        context.Request.EnableBuffering();
        using StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        string rawBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger
            .ForContext("rawBody", rawBody)
            .Information("Received SMSGlobal delivery receipt");

        SmsDeliveryReceipt? receipt = JsonSerializer.Deserialize<SmsDeliveryReceipt>(rawBody);

        if (receipt is null || string.IsNullOrWhiteSpace(receipt.OutgoingId) || string.IsNullOrWhiteSpace(receipt.Status))
        {
            _logger
                .ForContext(nameof(SmsDeliveryReceipt), receipt, true)
                .Warning("Received malformed SMSGlobal delivery receipt");

            return Results.BadRequest();
        }

        _logger
            .ForContext(nameof(SmsDeliveryReceipt), receipt, true)
            .Information("Delivery Receipt for Sms with OutgoingId: {OutgoingId}", receipt.OutgoingId);

        await mediator.Send(new RecordSmsDeliveryReceiptCommand(receipt));

        return Results.Ok("OK");
    }
}
