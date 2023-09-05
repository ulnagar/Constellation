namespace Constellation.Application.Periods.GetAllPeriods;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllPeriodsQuery()
    : IQuery<List<PeriodResponse>>;
