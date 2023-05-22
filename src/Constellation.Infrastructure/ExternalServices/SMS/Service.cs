﻿namespace Constellation.Infrastructure.ExternalServices.SMS;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Absences;
using Constellation.Core.ValueObjects;

public class Service : ISMSService
{
    private readonly ISMSGateway _service;
    private readonly ILinkShortenerGateway _linkShortenerService;

    public Service(ISMSGateway service, ILinkShortenerGateway linkShortenerService)
    {
        _service = service;
        _linkShortenerService = linkShortenerService;
    }

    public async Task<SMSMessageCollectionDto> SendAbsenceNotificationAsync(List<Absence> absences, List<PhoneNumber> phoneNumbers)
    {
        var classList = absences.Select(a => a.Offering.Name).Distinct().ToList();
        var classListString = "";
        foreach (var _class in classList.OrderBy(c => c))
            classListString += $"{_class}\r\n";

        var student = absences.First().Student;
        var date = absences.First().Date;

        var link = $"https://acos.aurora.nsw.edu.au/parents";
        link = await _linkShortenerService.ShortenURL(link);

        var messageText = $"{student.FirstName} was reported absent from the following classes on {date.ToShortDateString()}\r\n{classListString}To explain these absences, please click here {link}";

        var notifyUri = "json+https://acos.aurora.nsw.edu.au/api/SMS/Delivery";

        var messageContent = new SMSMessageToSend { origin = "Aurora", destinations = phoneNumbers.Select(number => number.ToString(PhoneNumber.Format.None)).ToList(), message = messageText, notifyUrl = notifyUri };
        return await _service.SendSmsAsync(messageContent);
    }
}
