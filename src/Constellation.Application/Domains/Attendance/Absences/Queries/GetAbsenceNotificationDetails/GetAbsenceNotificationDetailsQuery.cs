namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceNotificationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record GetAbsenceNotificationDetailsQuery(
    AbsenceId AbsenceId,
    AbsenceNotificationId NotificationId)
    : IQuery<string>;
