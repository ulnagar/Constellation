namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using System;

public sealed record TutorialTeacherResponse(
    Guid Id,
    string Name);
