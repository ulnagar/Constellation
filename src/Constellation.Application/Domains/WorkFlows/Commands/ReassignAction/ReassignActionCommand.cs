namespace Constellation.Application.Domains.WorkFlows.Commands.ReassignAction;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow.Identifiers;

public sealed record ReassignActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    StaffId StaffId)
    : ICommand;
