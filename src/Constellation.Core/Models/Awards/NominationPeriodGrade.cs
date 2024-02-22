namespace Constellation.Core.Models.Awards;

using Enums;
using Identifiers;

public sealed record NominationPeriodGrade(
    AwardNominationPeriodId PeriodId,
    Grade Grade);