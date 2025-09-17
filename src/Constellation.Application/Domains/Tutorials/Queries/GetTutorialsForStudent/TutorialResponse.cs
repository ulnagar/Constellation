namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialsForStudent;

using Core.Models.Timetables;
using Core.Models.Tutorials.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record TutorialResponse(
    TutorialId TutorialId,
    string Name,
    string Start,
    string End,
    List<Name> Teachers,
    List<Period> Periods);