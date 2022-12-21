namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using System;

public sealed record TutorialEnrolmentResponse(
    Guid Id,
    string Name,
    string Grade);
