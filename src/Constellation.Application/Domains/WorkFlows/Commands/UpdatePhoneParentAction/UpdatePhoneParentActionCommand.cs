namespace Constellation.Application.Domains.WorkFlows.Commands.UpdatePhoneParentAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;
using System;

public sealed record UpdatePhoneParentActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    string ParentName,
    string ParentNumber,
    DateTime DateOccurred,
    int IncidentNumber)
    : ICommand;