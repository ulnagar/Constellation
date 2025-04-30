namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record OutstandingAbsencesForSchoolResponse(
    AbsenceId AbsenceId,
    string StudentName,
    Grade StudentGrade,
    string AbsenceType,
    DateTime AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string OfferingName,
    AbsenceResponseId AbsenceResponseId);
