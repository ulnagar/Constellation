namespace Constellation.Application.Absences.GetAbsenceNotificationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceNotificationDetailsQuery(
    AbsenceId AbsenceId,
    AbsenceNotificationId NotificationId)
    : IQuery<string>;
