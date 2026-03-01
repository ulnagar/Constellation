namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

public class Service : ISMSService
{
    private readonly ISMSGateway _service;
    private readonly SMSGatewayConfiguration _configuration;

    private readonly string? _deliveryReceiptUri;

    public Service(
        ISMSGateway service,
        IOptions<SMSGatewayConfiguration> configuration,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _service = service;
        _configuration = configuration.Value;

        _deliveryReceiptUri = linkGenerator.GetUriByName(
            httpContextAccessor.HttpContext!,
            "SmsDeliveryReceipt",   // matches the .WithName() registration
            values: null);
    }

    public async Task<Result<SMSMessageCollectionDto>> SendAbsenceNotification(
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
        
        SMSMessageToSend messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
            destinations = phoneNumbers.Select(number => number.ToString(PhoneNumber.Format.None)).ToList(),
            message = messageText
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        return await _service.SendSmsAsync(messageContent);
    }

    public async Task<Result> SendLoginToken(
        string token,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default)
    {
        string messageText = $"Use token {token} for Aurora College Parent Portal. Token will expire in 10 mins.";

        SMSMessageToSend messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
            destinations = [phoneNumber.ToString(PhoneNumber.Format.None)],
            message = messageText
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        return await _service.SendSmsAsync(messageContent);
    }

    public async Task<Result<string>> SendEmergencyConsoleSms(
        AlertRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        SMSMessageToSend messageContent = new()
        {
            origin = _configuration.OutgoingNumber,
            destinations = [recipient.PhoneNumber.ToString(PhoneNumber.Format.None)],
            message = message
        };

        if (!string.IsNullOrWhiteSpace(_deliveryReceiptUri))
            messageContent.notifyUrl = $"json+{_deliveryReceiptUri}";

        Result<SMSMessageCollectionDto> result = await _service.SendSmsAsync(messageContent);

        if (result.IsFailure)
            return Result.Failure<string>(result.Error);

        return Result.Success(result.Value.Messages.First().OutgoingId);
    }
}
