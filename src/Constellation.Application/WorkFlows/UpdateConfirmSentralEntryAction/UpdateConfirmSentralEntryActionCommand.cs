namespace Constellation.Application.WorkFlows.UpdateConfirmSentralEntryAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record UpdateConfirmSentralEntryActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    bool Confirmed)
    : ICommand;