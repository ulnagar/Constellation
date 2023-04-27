namespace Constellation.Application.Awards.GetAwardCountsByTypeByGrade;

public sealed record AwardCountByTypeByGradeResponse(
    string ReportPeriod,
    string Grade,
    string AwardType,
    int Count);