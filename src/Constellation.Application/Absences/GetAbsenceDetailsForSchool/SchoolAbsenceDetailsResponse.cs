namespace Constellation.Application.Absences.GetAbsenceDetailsForSchool;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record SchoolAbsenceDetailsResponse(
    string StudentName,
    string ClassName,
    AbsenceId AbsenceId,
    DateTime AbsenceDate,
    string PeriodName,
    string AbsenceTimeframe);
