namespace Constellation.Infrastructure.ExternalServices.SMS;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Students;
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

    public async Task<SMSMessageCollectionDto> SendAbsenceNotification(
        List<AbsenceEntry> absences,
        Student student,
        List<PhoneNumber> phoneNumbers,
        CancellationToken cancellationToken = default)
    {
        string classListString = string.Empty;
        foreach (string offering in absences.Select(absence => absence.OfferingName).OrderBy(c => c))
            classListString += $"{offering}\r\n";

        string link = $"https://acos.aurora.nsw.edu.au/Parents";
        link = await _linkShortenerService.ShortenURL(link);

        string messageText = $"{student.FirstName} was reported absent from the following classes on {absences.First().Date.ToShortDateString()}\r\n{classListString}To explain these absences, please click here {link}";

        string notifyUri = "json+https://acos.aurora.nsw.edu.au/api/SMS/Delivery";

        SMSMessageToSend messageContent = new SMSMessageToSend
        {
            origin = "Aurora",
            destinations = phoneNumbers.Select(number => number.ToString(PhoneNumber.Format.None)).ToList(),
            message = messageText
        };

        return await _service.SendSmsAsync(messageContent);
    }
}
