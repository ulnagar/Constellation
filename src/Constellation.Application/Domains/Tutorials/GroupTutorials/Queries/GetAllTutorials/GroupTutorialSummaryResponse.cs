namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Queries.GetAllTutorials;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record GroupTutorialSummaryResponse(
    GroupTutorialId Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> Teachers,
    int Students);
