using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class SMSService : ISMSService, IScopedService
    {
        private readonly ISMSGateway _service;
        private readonly ILinkShortenerGateway _linkShortenerService;

        public SMSService(ISMSGateway service, ILinkShortenerGateway linkShortenerService)
        {
            _service = service;
            _linkShortenerService = linkShortenerService;
        }

        public async Task<SMSMessageCollectionDto> SendAbsenceNotificationAsync(List<Absence> absences, List<string> phoneNumbers)
        {
            var classList = absences.Select(a => a.Offering.Name).Distinct().ToList();
            var classListString = "";
            foreach (var _class in classList.OrderBy(c => c))
                classListString += $"{_class}\r\n";

            var student = absences.First().Student;
            var date = absences.First().Date;

            var link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Parents/{student.StudentId}";
            link = await _linkShortenerService.ShortenURL(link);

            var messageText = $"{student.FirstName} was reported absent from the following classes on {date.ToShortDateString()}\r\n{classListString}To explain these absences, please click here {link}";

            var notifyUri = "json+https://acos.aurora.nsw.edu.au/api/SMS/Delivery";

            var messageContent = new SMSMessageToSend { origin = "Aurora", destinations = phoneNumbers, message = messageText, notifyUrl = notifyUri };
            return await _service.SendSmsAsync(messageContent);
        }
    }
}
