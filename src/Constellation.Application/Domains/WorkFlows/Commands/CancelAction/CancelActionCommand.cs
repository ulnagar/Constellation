namespace Constellation.Application.Domains.WorkFlows.Commands.CancelAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record CancelActionCommand(
    CaseId CaseId,
    ActionId ActionId)
    : ICommand;