namespace Constellation.Application.Awards.GetStudentAwardStatistics;

using Constellation.Core.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record StudentAwardStatisticsResponse(
    StudentId StudentId,
    Name Name,
    Grade Grade,
    decimal AwardedAstras,
    decimal AwardedStellars,
    decimal AwardedGalaxies,
    decimal AwardedUniversals,
    decimal CalculatedStellars,
    decimal CalculatedGalaxies,
    decimal CalculatedUniversals);