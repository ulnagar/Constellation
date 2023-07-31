namespace Constellation.Application.Awards.GetNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetNominationPeriodRequest(
    AwardNominationPeriodId PeriodId)
    : IQuery<NominationPeriodDetailResponse>;