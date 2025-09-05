namespace Constellation.Core.Models.Awards;

using Core.Enums;
using Identifiers;

public sealed record NominationPeriodGrade(
    AwardNominationPeriodId PeriodId,
    Grade Grade);