namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Core.Models.Students.Identifiers;

public sealed record TutorialRollStudentResponse(
    StudentId StudentId,
    string Name,
    string Grade,
    bool Enrolled,
    bool Present);