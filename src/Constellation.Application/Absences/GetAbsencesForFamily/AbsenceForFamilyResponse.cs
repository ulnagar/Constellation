namespace Constellation.Application.Absences.GetAbsencesForFamily;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record AbsenceForFamilyResponse(
    AbsenceId Id,
    string StudentId,
    string StudentName,
    Grade StudentGrade,
    string AbsenceType,
    DateTime AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string AbsenceReason,
    string? OfferingName,
    string? Explanation,
    string? VerificationStatus,
    bool IsExplained,
    AbsenceForFamilyResponse.AbsenceStatus Status,
    bool CanParentExplainAbsence)
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