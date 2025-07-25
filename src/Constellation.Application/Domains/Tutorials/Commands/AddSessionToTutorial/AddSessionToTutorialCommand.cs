namespace Constellation.Application.Domains.Tutorials.Commands.AddSessionToTutorial;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Models.Tutorials.Identifiers;
using System;

public sealed record AddSessionToTutorialCommand(
    TutorialId Id,
    PeriodWeek Week,
    PeriodDay Day,
    TimeSpan StartTime,
    TimeSpan EndTime,
    StaffId StaffId)
    : ICommand;