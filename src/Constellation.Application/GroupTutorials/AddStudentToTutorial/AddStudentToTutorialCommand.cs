namespace Constellation.Application.GroupTutorials.AddStudentToTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record AddStudentToTutorialCommand(
    GroupTutorialId TutorialId,
    string StudentId,
    DateOnly? EffectiveTo) : ICommand;