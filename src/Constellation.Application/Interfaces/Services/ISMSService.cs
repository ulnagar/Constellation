﻿namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Constellation.Core.Models.Students;
using Constellation.Core.ValueObjects;
using Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISMSService
{
    Task<SMSMessageCollectionDto> SendAbsenceNotification(List<AbsenceEntry> absences, Student student, List<PhoneNumber> phoneNumbers, CancellationToken cancellationToken = default);
    Task SendLoginToken(string token, PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
}
