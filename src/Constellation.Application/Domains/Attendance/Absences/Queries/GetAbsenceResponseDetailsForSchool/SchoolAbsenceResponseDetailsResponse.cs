namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceResponseDetailsForSchool;

using Constellation.Core.Models.Absences.Identifiers;
using System;

public sealed record SchoolAbsenceResponseDetailsResponse(
    string StudentName,
    string ClassName,
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId,
    DateTime AbsenceDate,
    string PeriodName,
    string PeriodTimeframe,
    string AbsenceTimeframe,
    int AbsenceLength,
    string Explanation);