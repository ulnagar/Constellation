namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetAllNominationPeriods;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record NominationPeriodResponse(
    AwardNominationPeriodId PeriodId,
    string Name,
    DateOnly LockoutDate,
    List<Grade> IncludedGrades);