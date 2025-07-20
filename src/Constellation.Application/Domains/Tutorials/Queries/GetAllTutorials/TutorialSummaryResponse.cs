namespace Constellation.Application.Domains.Tutorials.Queries.GetAllTutorials;

using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using System.Collections.Generic;

public sealed record TutorialSummaryResponse(
    TutorialId TutorialId,
    TutorialName Name,
    List<string> Teachers,
    List<string> Students,
    decimal Duration, 
    bool IsCurrent);
