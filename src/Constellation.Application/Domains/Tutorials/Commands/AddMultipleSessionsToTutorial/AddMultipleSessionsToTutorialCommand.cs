namespace Constellation.Application.Domains.Tutorials.Commands.AddMultipleSessionsToTutorial;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Identifiers;
using System.Collections.Generic;

public sealed record AddMultipleSessionsToTutorialCommand(
    TutorialId TutorialId,
    StaffId StaffId,
    List<PeriodId> PeriodIds)
    : ICommand;
