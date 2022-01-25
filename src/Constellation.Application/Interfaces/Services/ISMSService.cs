using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ISMSService
    {
        Task<SMSMessageCollectionDto> SendAbsenceNotificationAsync(List<Absence> absences, List<string> phoneNumbers);
    }
}
