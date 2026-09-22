namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Core.ValueObjects;
using System;

public sealed record OutstandingAbsencesForSchoolResponse(
    AbsenceId AbsenceId,
    Name StudentName,
    Grade StudentGrade,
    string AbsenceType,
    DateTime AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string OfferingName,
    AbsenceResponseId AbsenceResponseId);
