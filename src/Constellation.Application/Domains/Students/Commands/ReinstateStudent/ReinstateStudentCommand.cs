namespace Constellation.Application.Domains.Students.Commands.ReinstateStudent;

using Constellation.Application.Abstractions.Messaging;
using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record ReinstateStudentCommand(
    StudentId StudentId,
    SchoolCode SchoolCode,
    Grade Grade)
    : ICommand;
