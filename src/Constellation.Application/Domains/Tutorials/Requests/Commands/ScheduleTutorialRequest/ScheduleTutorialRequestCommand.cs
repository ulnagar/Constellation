namespace Constellation.Application.Domains.Tutorials.Requests.Commands.ScheduleTutorialRequest;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Identifiers;
using System;
using System.Collections.Generic;

public sealed record ScheduleTutorialRequestCommand(
    RequestId RequestId,
    List<(PeriodId PeriodId, StaffId StaffId)> Periods,
    DateOnly StartDate,
    string Comment)
    : ICommand;
