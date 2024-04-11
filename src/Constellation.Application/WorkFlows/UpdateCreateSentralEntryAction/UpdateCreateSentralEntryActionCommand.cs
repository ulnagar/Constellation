namespace Constellation.Application.WorkFlows.UpdateCreateSentralEntryAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record UpdateCreateSentralEntryActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    bool NotRequired,
    int IncidentNumber)
    : ICommand;