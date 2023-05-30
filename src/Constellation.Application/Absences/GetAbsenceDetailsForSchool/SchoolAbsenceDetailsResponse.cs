namespace Constellation.Application.Absences.GetAbsenceDetailsForSchool;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record SchoolAbsenceDetailsResponse(
    Name StudentName,
    string ClassName,
    AbsenceId AbsenceId,
    DateOnly AbsenceDate,
    string PeriodName,
    string AbsenceTimeframe);
