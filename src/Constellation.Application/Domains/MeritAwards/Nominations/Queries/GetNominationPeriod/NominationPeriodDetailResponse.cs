namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNominationPeriod;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record NominationPeriodDetailResponse(
    string Name,
    DateOnly LockoutDate,
    List<Grade> IncludedGrades,
    List<NominationPeriodDetailResponse.NominationResponse> Nominations)
{
    public sealed record NominationResponse(
        AwardNominationId Id,
        Name Student,
        AwardType AwardType,
        string Description,
        string NominatedBy);
}