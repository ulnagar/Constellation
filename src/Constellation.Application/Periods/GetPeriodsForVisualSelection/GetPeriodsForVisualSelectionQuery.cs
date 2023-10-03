namespace Constellation.Application.Periods.GetPeriodsForVisualSelection;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetPeriodsForVisualSelectionQuery()
    : IQuery<List<PeriodVisualSelectResponse>>;