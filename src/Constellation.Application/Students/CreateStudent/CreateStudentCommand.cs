namespace Constellation.Application.Students.CreateStudent;

using Abstractions.Messaging;
using Core.Enums;

public sealed record CreateStudentCommand(
    string StudentId,
    string FirstName,
    string LastName,
    string Gender,
    Grade Grade,
    string PortalUsername,
    string SchoolCode)
    : ICommand;