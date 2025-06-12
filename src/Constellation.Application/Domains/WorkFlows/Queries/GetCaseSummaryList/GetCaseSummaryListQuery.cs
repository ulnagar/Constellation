namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseSummaryList;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public sealed record GetCaseSummaryListQuery(
    bool IsAdmin,
    StaffId CurrentUserId)
    : IQuery<List<CaseSummaryResponse>>;