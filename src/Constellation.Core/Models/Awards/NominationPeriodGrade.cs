namespace Constellation.Core.Models.Awards;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;

public sealed record NominationPeriodGrade(
    AwardNominationPeriodId PeriodId,
    Grade Grade);