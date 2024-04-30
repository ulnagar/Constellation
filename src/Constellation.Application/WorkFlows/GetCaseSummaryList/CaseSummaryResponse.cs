namespace Constellation.Application.WorkFlows.GetCaseSummaryList;

using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Core.ValueObjects;
using System;

public sealed record CaseSummaryResponse(
    CaseId CaseId,
    Name Subject,
    string Description,
    CaseStatus Status,
    DateTime CreatedAt,
    DateOnly DueDate,
    int TotalActions,
    int OutstandingActions);