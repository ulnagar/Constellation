namespace Constellation.Application.Domains.Timetables.Periods.Queries.GetPeriodsForVisualSelection;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetPeriodsForVisualSelectionQuery()
    : IQuery<List<PeriodVisualSelectResponse>>;