namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForStudent;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Core.Models.Students.Identifiers;
using System;

public sealed record AbsenceForStudentResponse(
    AbsenceId Id,
    StudentId StudentId,
    string StudentName,
    Grade StudentGrade,
    string AbsenceType,
    DateTime AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string AbsenceReason,
    string OfferingName,
    string Explanation,
    string VerificationStatus,
    bool IsExplained,
    AbsenceForStudentResponse.AbsenceStatus Status)
{
    public enum AbsenceStatus
    {
        VerifiedPartial,
        ExplainedWhole,
        UnexplainedPartial,
        UnverifiedPartial,
        UnexplainedWhole
    }
}
