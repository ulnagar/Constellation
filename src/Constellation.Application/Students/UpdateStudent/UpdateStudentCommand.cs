namespace Constellation.Application.Students.UpdateStudent;

using Abstractions.Messaging;
using Core.Enums;

public sealed record UpdateStudentCommand(
    string StudentId,
    string FirstName,
    string LastName,
    string PortalUsername,
    string AdobeConnectId,
    string SentralId,
    Grade CurrentGrade,
    Grade EnrolledGrade,
    string Gender,
    string SchoolCode)
    : ICommand;