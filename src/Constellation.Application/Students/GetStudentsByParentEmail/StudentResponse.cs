namespace Constellation.Application.Students.GetStudentsByParentEmail;

using Core.Models.Students.Identifiers;

public sealed record StudentResponse(
    StudentId StudentId,
    string FirstName,
    string LastName,
    string CurrentGrade)
{
    public string DisplayName => $"{FirstName} {LastName}";
}