using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

namespace Constellation.Application.Absences.GetAbsenceDetailsForParent;

public sealed record ParentAbsenceDetailsResponse(
    AbsenceId Id,
    Name Student,
    Grade Grade,
    AbsenceType Type,
    DateOnly Date,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string AbsenceReason,
    string OfferingName,
    string Reason,
    ResponseVerificationStatus VerificationStatus,
    string ValidatedBy,
    bool Explained,
    bool CanBeExplainedByParent);