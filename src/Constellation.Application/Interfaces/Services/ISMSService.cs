namespace Constellation.Application.Interfaces.Services;

using Constellation.Core.Models.Students;
using Constellation.Core.ValueObjects;
using Core.Shared;
using Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Domains.Messaging.Sms.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISMSService
{
    Task<Result<List<OutgoingSmsConfirmation>>> SendMessage(OutgoingSms message, CancellationToken cancellationToken);
    Task<Result<List<OutgoingSmsConfirmation>>> SendAbsenceNotification(List<AbsenceEntry> absences, Student student, List<PhoneNumber> phoneNumbers, CancellationToken cancellationToken = default);
    Task<Result> SendLoginToken(string token, PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
    Task<Result<string?>> SendEmergencyConsoleSms(AlertRecipient recipient, string message, CancellationToken cancellationToken = default);
}
