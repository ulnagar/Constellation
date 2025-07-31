namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForStudent;

using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Identifiers;
using System;

public sealed record AbsenceForStudentResponse(
    AbsenceId AbsenceId,
    Name Student,
    StudentId StudentId,
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


