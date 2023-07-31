namespace Constellation.Application.Awards.GetNominationPeriod;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record NominationPeriodDetailResponse(
    DateOnly LockoutDate,
    List<Grade> IncludedGrades,
    List<NominationPeriodDetailResponse.NominationResponse> Nominations)
{
    public sealed record NominationResponse(
        Name Student,
        AwardType AwardType,
        string Description,
        string NominatedBy);
}