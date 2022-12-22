namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

public sealed record TutorialRollStudentReponse(
    string StudentId,
    bool Enrolled,
    bool Present);