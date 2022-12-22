namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using System;

public sealed record TutorialRollResponse(
    Guid Id,
    DateOnly Date,
    bool Completed,
    int TotalStudents,
    int PresentStudents);