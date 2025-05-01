namespace Constellation.Application.Domains.WorkFlows.Commands.AddCaseDetailUpdateAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddCaseDetailUpdateActionCommand(
    CaseId CaseId,
    string StaffId,
    string Details)
    : ICommand;
