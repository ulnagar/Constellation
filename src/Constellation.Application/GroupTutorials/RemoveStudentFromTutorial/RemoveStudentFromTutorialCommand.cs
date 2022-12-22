namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record RemoveStudentFromTutorialCommand(
    Guid TutorialId,
    Guid EnrolmentId,
    DateOnly? EffectiveFrom) : ICommand;
