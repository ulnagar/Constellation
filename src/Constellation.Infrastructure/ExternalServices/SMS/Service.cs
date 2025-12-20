namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using System.Security.Cryptography;
using System.Text;

public class Service : ISMSService
{
    private readonly ISMSGateway _service;

    public Service(ISMSGateway service)
    {
        _service = service;
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
            origin = "Aurora",
            destinations = phoneNumbers.Select(number => number.ToString(PhoneNumber.Format.None)).ToList(),
            message = messageText
        };

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
            origin = "Aurora",
            destinations = [phoneNumber.ToString(PhoneNumber.Format.None)],
            message = messageText
        };

        return await _service.SendSmsAsync(messageContent);
    }

    public async Task<Result<string>> SendEmergencyConsoleSms(
        AlertRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        var id = GenerateRandomDigits(11);

        return Result.Success(id);
#endif

        SMSMessageToSend messageContent = new()
        {
            origin = "Aurora",
            destinations = [recipient.PhoneNumber.ToString(PhoneNumber.Format.None)],
            message = message
        };

        Result<SMSMessageCollectionDto> result = await _service.SendSmsAsync(messageContent);

        if (result.IsFailure)
            return Result.Failure<string>(result.Error);

        return Result.Success(result.Value.Messages.First().OutgoingId);
    }

    private string GenerateRandomDigits(int length)
    {
        // Use the secure RandomNumberGenerator
        var rng = RandomNumberGenerator.Create();
        var s = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            // For the first digit, ensure it's not zero (range 1-9 inclusive)
            if (i == 0)
            {
                s.Append(RandomNumberGenerator.GetInt32(1, 10).ToString());
            }
            // For subsequent digits, any number 0-9 is fine (range 0-10 exclusive)
            else
            {
                s.Append(RandomNumberGenerator.GetInt32(0, 10).ToString());
            }
        }
        return s.ToString();
    }
}
