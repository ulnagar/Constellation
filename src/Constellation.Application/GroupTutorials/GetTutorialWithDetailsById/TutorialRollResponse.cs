namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record TutorialRollResponse(
    TutorialRollId Id,
    DateOnly Date,
    bool Completed,
    int TotalStudents,
    int PresentStudents);