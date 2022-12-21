namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using System;
using System.Collections.Generic;

public sealed record GroupTutorialDetailResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<TutorialTeacherResponse> Teachers,
    IReadOnlyCollection<TutorialEnrolmentResponse> Students,
    IReadOnlyCollection<TutorialRollResponse> Rolls);