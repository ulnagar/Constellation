namespace Constellation.Application.Awards.GetRecentAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Awards.Models;
using System.Collections.Generic;

public sealed record GetRecentAwardsQuery(
    int Count)
    : IQuery<List<AwardResponse>>;
