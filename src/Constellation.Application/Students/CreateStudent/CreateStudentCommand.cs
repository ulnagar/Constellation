namespace Constellation.Application.Students.CreateStudent;

using Abstractions.Messaging;
using Core.Enums;

public sealed record CreateStudentCommand(
    string SRN,
    string FirstName,
    string PreferredName,
    string LastName,
    string Gender,
    Grade Grade,
    string EmailAddress,
    string SchoolCode)
    : ICommand;