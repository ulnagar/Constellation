namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateParentInterviewAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using System;
using System.Collections.Generic;

public sealed record UpdateParentInterviewActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    List<InterviewAttendee> Attendees,
    DateTime DateOccurred,
    int IncidentNumber)
    : ICommand;