namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

public sealed record TutorialRollStudentResponse(
    string StudentId,
    string Name,
    string Grade,
    bool Enrolled,
    bool Present);