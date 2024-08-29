namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using Core.Models.Students.Identifiers;

public sealed record TutorialRollStudentDetailsDto(
    StudentId StudentId,
    string Name,
    string Grade,
    bool Enrolled,
    bool Present);