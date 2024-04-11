namespace Constellation.Application.WorkFlows.AddSendEmailAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddSendEmailActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;