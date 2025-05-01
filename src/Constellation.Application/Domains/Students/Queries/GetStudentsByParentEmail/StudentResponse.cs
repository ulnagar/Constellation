namespace Constellation.Application.Domains.Students.Queries.GetStudentsByParentEmail;

using Core.Models.Students.Identifiers;

public sealed record StudentResponse(
    StudentId StudentId,
    string FirstName,
    string LastName,
    string CurrentGrade,
    bool ResidentialFamily)
{
    public string DisplayName => $"{FirstName} {LastName}";
}