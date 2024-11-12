namespace Constellation.Application.GroupTutorials.AddMultipleStudentsToTutorial;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record AddMultipleStudentsToTutorialCommand(
    GroupTutorialId TutorialId,
    List<StudentId> StudentIds)
    : ICommand;