namespace Constellation.Application.Domains.Timetables.Periods.Queries.GetAllPeriods;

using Core.Models.Timetables.Identifiers;

public sealed record PeriodResponse(
    PeriodId PeriodId,
    string Name,
    string Group,
    string SortOrder);
