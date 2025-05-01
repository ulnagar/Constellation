namespace Constellation.Application.Domains.GroupTutorials.Commands.AddStudentToTutorial;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System;

public sealed record AddStudentToTutorialCommand(
    GroupTutorialId TutorialId,
    StudentId StudentId,
    DateOnly? EffectiveTo) : ICommand;