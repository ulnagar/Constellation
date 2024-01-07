namespace Constellation.Application.Students.GetStudentsByParentEmail;

public sealed record StudentResponse(
    string StudentId,
    string FirstName,
    string LastName,
    string CurrentGrade)
{
    public string DisplayName => $"{FirstName} {LastName}";
}