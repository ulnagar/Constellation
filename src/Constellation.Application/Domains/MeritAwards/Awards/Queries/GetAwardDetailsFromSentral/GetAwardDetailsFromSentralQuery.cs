namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardDetailsFromSentral;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAwardDetailsFromSentralQuery()
    : IQuery<List<AwardDetailResponse>>;