namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetAllNominationPeriods;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllNominationPeriodsQuery()
    : IQuery<List<NominationPeriodResponse>>;