namespace Constellation.Application.Domains.WorkFlows.Commands.AddParentInterviewAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddParentInterviewActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;