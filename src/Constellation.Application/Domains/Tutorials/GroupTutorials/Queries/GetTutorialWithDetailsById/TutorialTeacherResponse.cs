namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Queries.GetTutorialWithDetailsById;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record TutorialTeacherResponse(
    TutorialTeacherId Id,
    string Name,
    DateOnly? Until);
