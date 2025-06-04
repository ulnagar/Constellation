namespace Constellation.Application.Domains.WorkFlows.Commands.AddSentralIncidentStatusAction;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddSentralIncidentStatusActionCommand(
    CaseId CaseId,
    StaffId StaffId)
    : ICommand;