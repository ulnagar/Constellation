namespace Constellation.Application.GroupTutorials.AddTeacherToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record AddTeacherToTutorialCommand(
    GroupTutorialId TutorialId,
    string StaffId,
    DateOnly? EffectiveTo) : ICommand;