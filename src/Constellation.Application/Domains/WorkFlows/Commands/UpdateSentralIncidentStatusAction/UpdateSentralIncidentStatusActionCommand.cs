namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateSentralIncidentStatusAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record UpdateSentralIncidentStatusActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    bool MarkResolved,
    bool MarkNotCompleted,
    int IncidentNumber)
    : ICommand;