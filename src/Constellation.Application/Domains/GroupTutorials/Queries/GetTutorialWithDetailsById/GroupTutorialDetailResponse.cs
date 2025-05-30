﻿namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialWithDetailsById;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record GroupTutorialDetailResponse(
    GroupTutorialId Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<TutorialTeacherResponse> Teachers,
    IReadOnlyCollection<TutorialEnrolmentResponse> Students,
    IReadOnlyCollection<TutorialRollResponse> Rolls);