namespace Constellation.Application.Absences.GetAbsenceDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record AbsenceDetailsResponse(
    AbsenceId AbsenceId,
    string StudentId,
    Name StudentName,
    Grade StudentGrade,
    string SchoolName,
    AbsenceType AbsenceType,
    DateOnly AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    List<AbsenceDetailsResponse.AbsenceNotificationDetails> Notifications,
    List<AbsenceDetailsResponse.AbsenceResponseDetails> Responses)
{
    public sealed record AbsenceResponseDetails(
        AbsenceResponseId ResponseId,
        ResponseType Type,
        string Explanation,
        ResponseVerificationStatus Status,
        DateTime ReceivedAt,
        string Verifier,
        string VerifierComment,
        DateTime? VerifiedAt);

    public sealed record AbsenceNotificationDetails(
        AbsenceNotificationId NotificationId,
        NotificationType Type,
        string Recipients,
        string Message,
        DateTime SentAt);
}