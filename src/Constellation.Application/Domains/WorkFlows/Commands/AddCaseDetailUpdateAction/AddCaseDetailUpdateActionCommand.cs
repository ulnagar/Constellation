namespace Constellation.Application.Domains.WorkFlows.Commands.AddCaseDetailUpdateAction;

using Abstractions.Messaging;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddCaseDetailUpdateActionCommand(
    CaseId CaseId,
    EmployeeId StaffId,
    string Details)
    : ICommand;
