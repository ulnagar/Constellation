namespace Constellation.Application.Domains.WorkFlows.Commands.AddSendEmailAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddSendEmailActionCommand(
    CaseId CaseId,
    string StaffId)
    : ICommand;