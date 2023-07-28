namespace Constellation.Application.Awards.GetAllNominationPeriods;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllNominationPeriodsQuery()
    : IQuery<List<NominationPeriodResponse>>;