namespace Constellation.Application.Absences.GetAbsenceSummaryForStudent;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record StudentAbsenceSummaryResponse(
    AbsenceId AbsenceId,
    bool IsExplained,
    AbsenceType AbsenceType,
    DateOnly AbsenceDate,
    string Timeframe,
    string PeriodName,
    string OfferingName);