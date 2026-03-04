namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.Domains.Messaging.Sms.Enums;
using Application.Domains.Messaging.Sms.Models;
using Application.Domains.Messaging.Sms.Repositories;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
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
            ? linkGenerator.GetUriByName(
                "SmsDeliveryReceipt",   // matches the .WithName() registration
                values: null,
                scheme: "https",
                host: new HostString("acos.aurora.nsw.edu.au"))
            : linkGenerator.GetUriByName(
                httpContextAccessor.HttpContext!,
                "SmsDeliveryReceipt",   // matches the .WithName() registration
                values: null);
    }

    public async Task<Result<List<OutgoingSmsConfirmation>>> SendAbsenceNotification(
        List<AbsenceEntry> absences,
        Student student,
        List<PhoneNumber> phoneNumbers,
        CancellationToken cancellationToken = default)
    {
        string classListString = string.Empty;
        foreach (string offering in absences.Select(absence => absence.OfferingName).OrderBy(c => c))
            classListString += $"{offering}\r\n";

        string link = $"http://edu.nsw.link/aurora";

        string messageText = $"{student.Name.PreferredName} was absent from the following classes on {absences.First().Date.ToShortDateString()}\r\n{classListString}To explain these absences, please click here {link}";
        
        OutgoingSms messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
            destinations = phoneNumbers.Select(number => number.ToString(PhoneNumber.Format.None)).ToList(),
            message = messageText
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        Result<List<OutgoingSmsConfirmation>> results = await _gateway.SendSms(messageContent, cancellationToken);

        if (results.IsFailure)
            return results;

        foreach (OutgoingSmsConfirmation confirmation in results.Value)
        {
            SmsMessage message = new()
            {
                SmsGlobalId = confirmation.Id ?? string.Empty,
                OutgoingId = confirmation.OutgoingId ?? string.Empty,
                From = confirmation.Origin ?? string.Empty,
                To = confirmation.Destination ?? string.Empty,
                Message = confirmation.Message ?? string.Empty,
                Direction = SmsDirection.Outbound,
                Status = SmsStatus.Sent,
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
