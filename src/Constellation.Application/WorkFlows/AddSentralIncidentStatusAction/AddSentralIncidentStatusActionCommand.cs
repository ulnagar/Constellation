namespace Constellation.Application.WorkFlows.AddSentralIncidentStatusAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddSentralIncidentStatusActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;