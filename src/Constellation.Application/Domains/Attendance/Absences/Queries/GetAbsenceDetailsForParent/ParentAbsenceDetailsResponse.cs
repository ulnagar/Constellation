using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using System;

namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForParent;

public sealed record ParentAbsenceDetailsResponse(
    AbsenceId Id,
    string Student,
    Grade Grade,
    string Type,
    DateTime Date,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string AbsenceReason,
    string OfferingName,
    string Reason,
    string VerificationStatus,
    string ValidatedBy,
    bool Explained,
    bool CanBeExplainedByParent);