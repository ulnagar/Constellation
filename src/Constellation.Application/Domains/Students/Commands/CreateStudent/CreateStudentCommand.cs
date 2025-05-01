namespace Constellation.Application.Domains.Students.Commands.CreateStudent;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Students.Enums;

public sealed record CreateStudentCommand(
    string SRN,
    string FirstName,
    string PreferredName,
    string LastName,
    Gender Gender,
    Grade Grade,
    string EmailAddress,
    string SchoolCode)
    : ICommand;