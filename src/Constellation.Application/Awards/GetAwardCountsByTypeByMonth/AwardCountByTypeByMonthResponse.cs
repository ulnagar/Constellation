namespace Constellation.Application.Awards.GetAwardCountsByTypeByMonth;

public sealed record AwardCountByTypeByMonthResponse(
    string MonthName,
    string SortValue,
    string AwardType,
    int Count);