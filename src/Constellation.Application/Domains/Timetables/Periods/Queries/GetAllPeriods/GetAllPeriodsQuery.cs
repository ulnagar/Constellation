namespace Constellation.Application.Domains.Timetables.Periods.Queries.GetAllPeriods;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllPeriodsQuery()
    : IQuery<List<PeriodResponse>>;
