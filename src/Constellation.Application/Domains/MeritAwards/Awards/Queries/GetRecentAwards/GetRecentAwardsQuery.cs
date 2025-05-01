namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetRecentAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.MeritAwards.Awards.Models;
using System.Collections.Generic;

public sealed record GetRecentAwardsQuery(
    int Count)
    : IQuery<List<AwardResponse>>;
