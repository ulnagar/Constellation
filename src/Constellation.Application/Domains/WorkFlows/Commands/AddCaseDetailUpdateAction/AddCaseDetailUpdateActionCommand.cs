namespace Constellation.Application.Domains.WorkFlows.Commands.AddCaseDetailUpdateAction;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddCaseDetailUpdateActionCommand(
    CaseId CaseId,
    StaffId StaffId,
    string Details)
    : ICommand;
