namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseById;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record GetCaseByIdQuery(
    CaseId CaseId)
    : IQuery<CaseDetailsResponse>;
