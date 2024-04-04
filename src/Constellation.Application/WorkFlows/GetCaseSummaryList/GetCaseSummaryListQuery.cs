namespace Constellation.Application.WorkFlows.GetCaseSummaryList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCaseSummaryListQuery()
    : IQuery<List<CaseSummaryResponse>>;