namespace Constellation.Application.Absences.GetAbsenceResponseDetailsForSchool;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
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