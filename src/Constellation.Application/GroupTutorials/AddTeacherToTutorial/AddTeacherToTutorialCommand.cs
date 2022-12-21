namespace Constellation.Application.GroupTutorials.AddTeacherToTutorial;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record AddTeacherToTutorialCommand(
    Guid TutorialId,
    string StaffId,
    DateOnly? EffectiveTo) : ICommand;