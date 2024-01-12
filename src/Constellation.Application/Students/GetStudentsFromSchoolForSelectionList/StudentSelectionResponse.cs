namespace Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;

public sealed record StudentSelectionResponse(
    string StudentId,
    string FirstName,
    string LastName,
    string CurrentGrade)
{
    public string DisplayName => $"{FirstName} {LastName}";
}