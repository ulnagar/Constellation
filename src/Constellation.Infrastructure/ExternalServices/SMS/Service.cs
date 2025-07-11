﻿namespace Constellation.Infrastructure.ExternalServices.SMS;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;

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

        string link = $"https://edu.nsw.link/aurora";

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
}
