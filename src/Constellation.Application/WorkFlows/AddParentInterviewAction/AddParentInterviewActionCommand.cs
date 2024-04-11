namespace Constellation.Application.WorkFlows.AddParentInterviewAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddParentInterviewActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;