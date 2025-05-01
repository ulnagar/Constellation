namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseSummaryList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCaseSummaryListQuery(
    bool IsAdmin,
    string CurrentUserId)
    : IQuery<List<CaseSummaryResponse>>;