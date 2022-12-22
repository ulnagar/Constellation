namespace Constellation.Application.GroupTutorials.RemoveTeacherFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record RemoveTeacherFromTutorialCommand(
    Guid TutorialId, 
    Guid TeacherId,
    DateOnly? TakesEffectOn = null) : ICommand;
