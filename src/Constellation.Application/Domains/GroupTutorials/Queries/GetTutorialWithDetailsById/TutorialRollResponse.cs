namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialWithDetailsById;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record TutorialRollResponse(
    TutorialRollId Id,
    DateOnly Date,
    bool Completed,
    int TotalStudents,
    int PresentStudents);