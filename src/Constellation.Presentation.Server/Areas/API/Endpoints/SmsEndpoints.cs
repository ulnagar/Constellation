namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;
using Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;
using Constellation.Application.Domains.Messaging.Sms.Models;
using MediatR;
using Serilog;

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

    private static async Task<IResult> HandleIncomingSms(IncomingSms message, ISender mediator)
    {
        if (string.IsNullOrWhiteSpace(message.From) || string.IsNullOrWhiteSpace(message.Msg))
        {
            _logger
                .ForContext(nameof(IncomingSms), message, true)
                .Warning("Received malformed SMSGlobal postback");

            return Results.BadRequest();
        }

        _logger
            .ForContext(nameof(IncomingSms), message, true)
            .Information("Incoming SMS From: {From}", message.From);

        await mediator.Send(new CreateNewIncomingSmsRecordCommand(message));

        return Results.Ok("OK");
    }

    private static async Task<IResult> HandleDeliveryReceipt(SmsDeliveryReceipt receipt, ISender mediator)
    {
        if (string.IsNullOrWhiteSpace(receipt.Id) || string.IsNullOrWhiteSpace(receipt.Status))
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
