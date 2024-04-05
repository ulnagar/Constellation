namespace Constellation.Application.WorkFlows.CloseCase;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record CloseCaseCommand(
    CaseId CaseId)
    : ICommand;