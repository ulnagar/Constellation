namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardCountsByTypeByGrade;

public sealed record AwardCountByTypeByGradeResponse(
    string ReportPeriod,
    string Grade,
    string AwardType,
    int Count);