namespace Constellation.Application.Domains.Enrolments.Commands.UnenrolStudentFromTutorial;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.Models.Tutorials.Identifiers;

public sealed record UnenrolStudentFromTutorialCommand(
    StudentId StudentId,
    TutorialId TutorialId)
    :ICommand;