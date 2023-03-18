namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISMSService
{
    Task<SMSMessageCollectionDto> SendAbsenceNotificationAsync(List<Absence> absences, List<PhoneNumber> phoneNumbers);
}
