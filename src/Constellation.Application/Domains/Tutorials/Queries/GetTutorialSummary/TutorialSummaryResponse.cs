namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialSummary;

using Constellation.Core.Enums;
using Core.Models.Tutorials.Identifiers;
using System;
using System.Collections.Generic;

public sealed record TutorialSummaryResponse(
    TutorialId Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> Teachers,
    int MinPerFN,
    bool IsActive);
