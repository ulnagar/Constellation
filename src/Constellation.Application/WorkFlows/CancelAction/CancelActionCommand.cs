namespace Constellation.Application.WorkFlows.CancelAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record CancelActionCommand(
    CaseId CaseId,
    ActionId ActionId)
    : ICommand;