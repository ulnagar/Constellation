namespace Constellation.Application.Domains.Tutorials.Commands.AddSessionToTutorial;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Identifiers;

public sealed record AddSessionToTutorialCommand(
    TutorialId Id,
    PeriodId PeriodId,
    StaffId StaffId)
    : ICommand;