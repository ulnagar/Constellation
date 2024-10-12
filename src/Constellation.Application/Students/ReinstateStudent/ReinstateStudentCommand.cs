namespace Constellation.Application.Students.ReinstateStudent;

using Constellation.Application.Abstractions.Messaging;
using Core.Enums;
using Core.Models.Students.Identifiers;

public sealed record ReinstateStudentCommand(
    StudentId StudentId,
    string SchoolCode,
    Grade Grade)
    : ICommand;
