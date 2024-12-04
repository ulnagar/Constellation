namespace Constellation.Application.Periods.GetAllPeriods;

using Core.Models.Timetables.Identifiers;

public sealed record PeriodResponse(
    PeriodId PeriodId,
    string Name,
    string Group);
