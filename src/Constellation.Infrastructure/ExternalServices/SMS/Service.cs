namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
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
    
    public async Task<Result<List<OutgoingSmsConfirmation>>> SendMessage(
        OutgoingSms message,
        CancellationToken cancellationToken) =>
        await _gateway.SendSms(message, cancellationToken);

    public async Task<Result<List<OutgoingSmsConfirmation>>> SendAbsenceNotification(
        List<AbsenceEntry> absences,
        Student student,
        List<SmsRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        string classListString = string.Empty;
        foreach (string offering in absences.Select(absence => absence.OfferingName).OrderBy(c => c))
            classListString += $"{offering}\r\n";

        string link = $"http://edu.nsw.link/aurora";

        string messageText = $"{student.Name.PreferredName} was absent from the following classes on {absences.First().Date.ToShortDateString()}\r\n{classListString}To explain these absences, please click here {link}";

        List<string> destinations = recipients.Select(recipient => recipient.Number).ToList();

        OutgoingSms messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
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

            SmsRecipient receiver = recipients
                .FirstOrDefault(recipient => 
                    recipient.Number == recipientPhoneNumber.Value.ToString(PhoneNumber.Format.None)) 
                ?? SmsRecipient.Unknown;

            SmsMessage message = new()
            {
                SmsGlobalId = confirmation.Id ?? string.Empty,
                SendingModule = "Absences",
                OutgoingId = confirmation.OutgoingId ?? string.Empty,
                Sender = sender,
                Recipient = receiver,
                Message = confirmation.Message ?? string.Empty,
                Direction = MessageDirection.Outbound,
                Status = MessageStatus.Sent,
                CreatedAt = confirmation.DateTime
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

    public async Task<Result<string?>> SendEmergencyConsoleSms(
        AlertRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (recipient.PhoneNumber == PhoneNumber.Empty)
            return Result.Failure<string?>(SmsRecipientErrors.NumberEmpty);

        OutgoingSms messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
            destinations = [recipient.PhoneNumber.ToString(PhoneNumber.Format.None)],
            message = message
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        Result<List<OutgoingSmsConfirmation>> result = await _gateway.SendSms(messageContent, cancellationToken);

        if (result.IsFailure)
            return Result.Failure<string?>(result.Error);

        return Result.Success(result.Value.First().OutgoingId);
    }
}
