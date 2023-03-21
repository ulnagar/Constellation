namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record TutorialEnrolmentResponse(
    TutorialEnrolmentId Id,
    string Name,
    string Grade,
    DateOnly? Until);
