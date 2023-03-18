using System;

namespace Constellation.Application.Absences.GetAbsenceSummaryForStudent;

public sealed record StudentAbsenceSummaryResponse(
    Guid AbsenceId,
    bool IsExplained,
    string AbsenceType,
    DateOnly AbsenceDate,
    string Timeframe,
    string PeriodName,
    string OfferingName);