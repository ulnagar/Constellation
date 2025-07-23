namespace Constellation.Application.Domains.Enrolments.Commands.EnrolStudentInTutorial;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.Models.Tutorials.Identifiers;

public sealed record EnrolStudentInTutorialCommand(
    StudentId StudentId,
    TutorialId TutorialId)
    : ICommand;