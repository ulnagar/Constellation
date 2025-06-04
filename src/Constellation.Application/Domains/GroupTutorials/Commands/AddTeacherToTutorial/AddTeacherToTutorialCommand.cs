namespace Constellation.Application.Domains.GroupTutorials.Commands.AddTeacherToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using System;

public sealed record AddTeacherToTutorialCommand(
    GroupTutorialId TutorialId,
    StaffId StaffId,
    DateOnly? EffectiveTo) : ICommand;