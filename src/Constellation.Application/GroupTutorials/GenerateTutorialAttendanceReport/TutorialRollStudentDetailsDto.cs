namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

public sealed record TutorialRollStudentDetailsDto(
    string StudentId,
    string Name,
    string Grade,
    bool Enrolled,
    bool Present);