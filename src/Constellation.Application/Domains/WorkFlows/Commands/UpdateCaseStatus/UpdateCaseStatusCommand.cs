namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateCaseStatus;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;

public sealed record UpdateCaseStatusCommand(
    CaseId CaseId,
    CaseStatus Status)
    : ICommand;