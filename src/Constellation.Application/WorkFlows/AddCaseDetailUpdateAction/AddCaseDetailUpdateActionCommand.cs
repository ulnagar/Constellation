namespace Constellation.Application.WorkFlows.AddCaseDetailUpdateAction;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.WorkFlow.Identifiers;

public sealed record AddCaseDetailUpdateActionCommand(
    CaseId CaseId,
    string StaffId,
    string Details)
    : ICommand;
