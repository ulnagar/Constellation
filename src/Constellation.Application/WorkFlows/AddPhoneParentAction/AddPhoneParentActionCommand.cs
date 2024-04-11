namespace Constellation.Application.WorkFlows.AddPhoneParentAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddPhoneParentActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;
