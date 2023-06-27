namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISMSService
{
    Task<SMSMessageCollectionDto> SendAbsenceNotification(List<AbsenceEntry> absences, Student student, List<PhoneNumber> phoneNumbers, CancellationToken cancellationToken = default);
}
