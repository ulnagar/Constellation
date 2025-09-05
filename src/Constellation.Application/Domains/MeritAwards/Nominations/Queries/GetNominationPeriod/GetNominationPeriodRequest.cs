namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Awards.Identifiers;

public sealed record GetNominationPeriodRequest(
    AwardNominationPeriodId PeriodId)
    : IQuery<NominationPeriodDetailResponse>;