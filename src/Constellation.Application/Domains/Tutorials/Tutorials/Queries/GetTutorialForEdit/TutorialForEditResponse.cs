namespace Constellation.Application.Domains.Tutorials.Tutorials.Queries.GetTutorialForEdit;

using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using System;

public sealed record TutorialForEditResponse(
    TutorialId TutorialId,
    TutorialName Name,
    DateOnly StartDate,
    DateOnly EndDate);
