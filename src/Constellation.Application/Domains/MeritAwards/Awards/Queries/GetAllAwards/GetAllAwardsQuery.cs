namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAllAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.MeritAwards.Awards.Models;
using System.Collections.Generic;

public sealed record GetAllAwardsQuery(
    bool CurrentYearOnly)
    : IQuery<List<AwardResponse>>;