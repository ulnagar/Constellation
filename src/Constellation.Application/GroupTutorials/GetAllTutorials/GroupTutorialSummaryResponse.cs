namespace Constellation.Application.GroupTutorials.GetAllTutorials;

using System;
using System.Collections.Generic;

public sealed record GroupTutorialSummaryResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> Teachers,
    List<string> Students);
