namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;

using Core.Enums;
using Core.Models.Students.Identifiers;

public sealed record StudentSelectionResponse(
    StudentId StudentId,
    string FirstName,
    string LastName,
    Grade CurrentGrade)
{
    public string DisplayName => $"{FirstName} {LastName}";
}