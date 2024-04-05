namespace Constellation.Application.WorkFlows.ReassignAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record ReassignActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    string StaffId)
    : ICommand;
