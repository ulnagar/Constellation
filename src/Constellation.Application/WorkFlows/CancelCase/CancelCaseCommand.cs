namespace Constellation.Application.WorkFlows.CancelCase;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record CancelCaseCommand(
    CaseId CaseId)
    : ICommand;