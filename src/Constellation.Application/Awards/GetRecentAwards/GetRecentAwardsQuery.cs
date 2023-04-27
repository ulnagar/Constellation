namespace Constellation.Application.Awards.GetRecentAwards;

using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record GetRecentAwardsQuery(
    int Count)
    : IQuery<List<RecentAwardResponse>>;
