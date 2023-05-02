namespace Constellation.Application.Awards.GetAllAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Awards.Models;
using System.Collections.Generic;

public sealed record GetAllAwardsQuery(
    bool CurrentYearOnly)
    : IQuery<List<AwardResponse>>;