namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardCountsByTypeByMonth;

public sealed record AwardCountByTypeByMonthResponse(
    string MonthName,
    string SortValue,
    string AwardType,
    int Count);