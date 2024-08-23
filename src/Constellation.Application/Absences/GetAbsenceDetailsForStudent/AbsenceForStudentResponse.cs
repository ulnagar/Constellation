namespace Constellation.Application.Absences.GetAbsenceDetailsForStudent;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record AbsenceForStudentResponse(
    AbsenceId AbsenceId,
    Name Student,
    string StudentId,
    OfferingId OfferingId,
    string OfferingName,
    DateOnly AbsenceDate,
    AbsenceType AbsenceType,
    string PeriodName,
    string PeriodTimeframe,
    string AbsenceTimeframe,
    int AbsenceLength,
    string AbsenceReason,
    string Reason,
    string VerificationStatus,
    string ValidatedBy,
    bool Explained);


