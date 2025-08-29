namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseSummaryList;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow.Enums;
using System.Collections.Generic;

public sealed record GetCaseSummaryListQuery(
    bool IsAdmin,
    StaffId CurrentUserId,
    CaseStatusFilter Filter)
    : IQuery<List<CaseSummaryResponse>>;