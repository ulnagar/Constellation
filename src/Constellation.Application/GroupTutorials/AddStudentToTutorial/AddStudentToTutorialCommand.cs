namespace Constellation.Application.GroupTutorials.AddStudentToTutorial;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record AddStudentToTutorialCommand(
    Guid TutorialId,
    string StudentId,
    DateOnly? EffectiveTo) : ICommand;