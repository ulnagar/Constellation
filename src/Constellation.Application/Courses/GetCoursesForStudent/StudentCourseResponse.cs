namespace Constellation.Application.Courses.GetCoursesForStudent;

using Core.Models.Subjects.Identifiers;

public sealed record StudentCourseResponse(
    CourseId Id,
    string Name,
    string Grade)
{
    public string DisplayName => $"{Grade} {Name}";
}