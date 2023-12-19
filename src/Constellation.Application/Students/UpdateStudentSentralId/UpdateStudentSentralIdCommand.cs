namespace Constellation.Application.Students.UpdateStudentSentralId;

using Abstractions.Messaging;

public sealed record UpdateStudentSentralIdCommand(
        string StudentId)
    : ICommand;