namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Messaging.Sms.Dtos;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Errors;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Repositories;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Globalization;

public sealed class Service : ISMSService
{
    private readonly ISMSGateway _gateway;
    private readonly ISmsRepository _smsRepository;
    private readonly SMSGatewayConfiguration _configuration;

    private readonly string? _deliveryReceiptUri;

    public Service(
        ISMSGateway gateway,
        IOptions<SMSGatewayConfiguration> configuration,
        ISmsRepository smsRepository,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _gateway = gateway;
        _smsRepository = smsRepository;
        _configuration = configuration.Value;

        _deliveryReceiptUri = httpContextAccessor.HttpContext == null 
            ? "https://acos.aurora.nsw.edu.au/api/sms"
            : linkGenerator.GetUriByName(
                httpContextAccessor.HttpContext!,
                "SmsDeliveryReceipt",   // matches the .WithName() registration
                values: null);
    }
    
    public async Task<Result> SendQueuedMessage(
        MessageSender sender,
        SmsRecipient receiver,
        string messageBody,
        CancellationToken cancellationToken = default)
    {
        OutgoingSms message = new()
        {
            origin = sender.Destination, 
            destinations = [receiver.Number], 
            message = messageBody
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            message.notifyUrl = $"json+{_deliveryReceiptUri}";

        Result<List<OutgoingSmsConfirmation>> results = await _gateway.SendSms(message, cancellationToken);

        if (results.IsFailure)
            return results;

        foreach (OutgoingSmsConfirmation confirmation in results.Value)
        {
            SmsMessage messageRecord = new(
                "Messaging",
                confirmation.Id ?? string.Empty,
                sender,
                receiver,
                confirmation.Message ?? string.Empty,
                MessageDirection.Outbound,
                MessageStatus.Sent,
                confirmation.DateTime) { OutgoingId = confirmation.OutgoingId ?? string.Empty, };

            _smsRepository.Insert(messageRecord);
        }

        return results;
    }

    public async Task<Result<List<OutgoingSmsConfirmation>>> SendAbsenceNotification(
        DateOnly absenceDate,
        Student student,
        List<SmsRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        const string link = $"http://edu.nsw.link/aurora";

        string messageText = $"{student.Name.PreferredName} was absent from classes on {absenceDate.ToShortDateString()}. To explain these absences, please login at {link} or reply using the code {absenceDate.ToString("ddMM", DateTimeFormatInfo.InvariantInfo)}";

        List<string> destinations = recipients.Select(recipient => recipient.Number).ToList();

        OutgoingSms messageContent = new()
        {
            origin = SmsRecipient.Aurora.Number,
            destinations = destinations,
            message = messageText
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        Result<List<OutgoingSmsConfirmation>> results = await _gateway.SendSms(messageContent, cancellationToken);

        if (results.IsFailure)
            return results;

        foreach (OutgoingSmsConfirmation confirmation in results.Value)
        {
            SmsRecipient sender = SmsRecipient.Unknown;

            if (confirmation.Origin == SmsRecipient.AuroraNoReply.Number)
                sender = SmsRecipient.AuroraNoReply;

            if (confirmation.Origin == SmsRecipient.Aurora.Number)
                sender = SmsRecipient.Aurora;

            Result<PhoneNumber> recipientPhoneNumber = PhoneNumber.Create(confirmation.Destination ?? string.Empty);

            SmsRecipient receiver = recipientPhoneNumber.IsFailure 
                ? SmsRecipient.Unknown 
                : recipients.FirstOrDefault(recipient => 
                    recipient.Number == recipientPhoneNumber.Value.ToString(PhoneNumber.Format.None)) 
                    ?? SmsRecipient.Unknown;

            SmsMessage message = new(
                "Absences",
                confirmation.Id ?? string.Empty,
                sender,
                receiver,
                confirmation.Message ?? string.Empty,
                MessageDirection.Outbound,
                MessageStatus.Sent,
                confirmation.DateTime)
            {
                OutgoingId = confirmation.OutgoingId ?? string.Empty,
            };

            _smsRepository.Insert(message);
        }

        return results;
    }

    public async Task<Result> SendLoginToken(
        string token,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default)
    {
        string messageText = $"Use token {token} for Aurora College Parent Portal. Token will expire in 10 mins.";

        if (phoneNumber == PhoneNumber.Empty)
            return Result.Failure(SmsRecipientErrors.NumberEmpty);

        OutgoingSms messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
            destinations = [phoneNumber.ToString(PhoneNumber.Format.None)],
            message = messageText
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        return await _gateway.SendSms(messageContent, cancellationToken);
    }
}
