namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseById;

using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using System;
using System.Collections.Generic;

public sealed record CaseDetailsResponse(
    CaseId CaseId,
    string CaseDescription,
    CaseStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<CaseDetailsResponse.CaseActionSummary> Actions)
{
    public sealed record CaseActionSummary(
        ActionId ActionId,
        ActionId? ParentActionId,
        DateTime CreatedAt);

}