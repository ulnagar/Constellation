namespace Constellation.Application.Periods.GetAllPeriods;

public sealed record PeriodResponse(
    int PeriodId,
    string Name,
    string Group);
