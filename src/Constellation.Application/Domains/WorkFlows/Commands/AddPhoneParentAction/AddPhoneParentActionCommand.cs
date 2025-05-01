namespace Constellation.Application.Domains.WorkFlows.Commands.AddPhoneParentAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddPhoneParentActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;
