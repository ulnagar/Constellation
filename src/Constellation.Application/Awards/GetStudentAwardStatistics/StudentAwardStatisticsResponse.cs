namespace Constellation.Application.Awards.GetStudentAwardStatistics;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;

public sealed record StudentAwardStatisticsResponse(
    string StudentId,
    Name Name,
    Grade Grade,
    decimal AwardedAstras,
    decimal AwardedStellars,
    decimal AwardedGalaxies,
    decimal AwardedUniversals,
    decimal CalculatedStellars,
    decimal CalculatedGalaxies,
    decimal CalculatedUniversals);