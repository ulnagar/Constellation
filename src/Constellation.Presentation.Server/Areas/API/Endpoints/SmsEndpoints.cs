namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;
using Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;
using Constellation.Application.Domains.Messaging.Sms.Models;
using Core.Shared;
using MediatR;
using Models;
using Serilog;
using Shared.Helpers.Logging;
using System.Text;
using System.Text.Json;

public static class SmsEndpoints
{
    private static readonly Serilog.ILogger _logger = Log.Logger.ForContext(typeof(SmsEndpoints)).ForContext(LogDefaults.Application, LogDefaults.StaffPortal);

    public static void MapSmsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sms/incoming", HandleIncomingSms)
            .WithName("SmsIncoming")
            .Accepts<IncomingSms>("application/json");

        app.MapPost("/api/sms/delivery-receipt", HandleDeliveryReceipt)
            .WithName("SmsDeliveryReceipt")
            .Accepts<SmsDeliveryReceipt>("application/json");
    }

    private static async Task<IResult> HandleIncomingSms(
        [AsParameters] SmsGlobalIncomingMessage message,
        ISender mediator)
    {
        if (string.IsNullOrWhiteSpace(message.From) || string.IsNullOrWhiteSpace(message.Msg))
        {
            _logger
                .ForContext(nameof(SmsGlobalIncomingMessage), message, true)
                .Warning("Received malformed Incoming SMS");

            return Results.BadRequest();
        }

        _logger
            .ForContext(nameof(SmsGlobalIncomingMessage), message, true)
            .Information("Incoming SMS From: {From}", message.From);

        await mediator.Send(new CreateNewIncomingSmsRecordCommand(message.ToModel()));

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

        if (receipt is null 
            || string.IsNullOrWhiteSpace(receipt.OutgoingId) 
            || string.IsNullOrWhiteSpace(receipt.Status)
            || receipt.MessageIds.Count == 0)
        {
            _logger
                .ForContext("RawBody", rawBody)
                .ForContext(nameof(SmsDeliveryReceipt), receipt, true)
                .Warning("Received malformed SMSGlobal delivery receipt");

            return Results.BadRequest();
        }

        _logger
            .ForContext(nameof(SmsDeliveryReceipt), receipt, true)
            .Information("Delivery Receipt for Sms with OutgoingId: {OutgoingId}", receipt.OutgoingId);

        Result result = await mediator.Send(new RecordSmsDeliveryReceiptCommand(receipt));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(SmsDeliveryReceipt), receipt, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to record delivery receipt for Sms");

            return Results.BadRequest();
        }

        return Results.Ok("OK");
    }
}
