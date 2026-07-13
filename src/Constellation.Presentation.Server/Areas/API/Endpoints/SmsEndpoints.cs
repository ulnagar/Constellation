namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;
using Application.Domains.Messaging.Sms.Dtos;
using Application.Interfaces.Services;
using Core.Models.Messaging.Tracking;
using MediatR;
using Models;
using Serilog;
using Shared.Extensions;
using System.Text;
using System.Text.Json;

public static class SmsEndpoints
{
    private static readonly Serilog.ILogger _logger = Log.Logger.ForContext(typeof(SmsEndpoints)).ForStaffPortal();

    public static void MapSmsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sms", HandleIncomingSms)
            .WithName("SmsIncoming");

        app.MapPost("/api/sms", HandleDeliveryReceipt)
            .WithName("SmsDeliveryReceipt")
            .Accepts<SmsDeliveryReceipt>("application/json");
    }

    private static async Task<IResult> HandleIncomingSms(
        [AsParameters] SmsGlobalIncomingMessage message,
        ISender mediator)
    {
        _logger
            .ForContext(nameof(SmsGlobalIncomingMessage), message, true)
            .Information("Received SMSGlobal incoming message");

        if (string.IsNullOrWhiteSpace(message.From) || string.IsNullOrWhiteSpace(message.Msg))
        {
            _logger
                .ForContext(nameof(SmsGlobalIncomingMessage), message, true)
                .Warning("Received malformed Incoming SMS");

            return Results.BadRequest();
        }

        await mediator.Send(new CreateNewIncomingSmsRecordCommand(message.ToModel()));

        return Results.Ok("OK");
    }

    private static async Task<IResult> HandleDeliveryReceipt(
        ITrackingEventQueueService queue,
        HttpContext context)
    {
        context.Request.EnableBuffering();
        using StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        string rawBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger
            .ForContext("rawBody", rawBody)
            .Information("Received SMSGlobal delivery receipt");

        SmsDeliveryReceipt? receipt = JsonSerializer.Deserialize<SmsDeliveryReceipt>(rawBody);

        if (receipt is null || string.IsNullOrWhiteSpace(receipt.OutgoingId))
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

        await queue.EnqueueAsync(new SmsDeliveryReceiptEvent(receipt.OutgoingId, receipt.Status, receipt.DateTime));

        return Results.Ok("OK");
    }
}
