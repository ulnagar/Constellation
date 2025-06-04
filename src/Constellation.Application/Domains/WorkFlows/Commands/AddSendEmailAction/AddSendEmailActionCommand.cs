namespace Constellation.Application.Domains.WorkFlows.Commands.AddSendEmailAction;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddSendEmailActionCommand(
    CaseId CaseId,
    StaffId StaffId)
    : ICommand;