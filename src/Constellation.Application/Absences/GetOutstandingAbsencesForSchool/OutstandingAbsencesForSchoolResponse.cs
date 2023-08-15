namespace Constellation.Application.Absences.GetOutstandingAbsencesForSchool;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record OutstandingAbsencesForSchoolResponse(
    AbsenceId AbsenceId,
    string StudentName,
    Grade StudentGrade,
    string AbsenceType,
    DateOnly AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    int AbsenceLength,
    string AbsenceTimeframe,
    string OfferingName,
    AbsenceResponseId? AbsenceResponseId);
