namespace Constellation.Application.Students.ReinstateStudent;

using Constellation.Application.Abstractions.Messaging;

public sealed record ReinstateStudentCommand(
    string StudentId)
    : ICommand;
