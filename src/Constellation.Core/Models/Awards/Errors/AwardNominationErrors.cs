namespace Constellation.Core.Models.Awards.Errors;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;

public static class AwardNominationErrors
{
    public static readonly Error NotRecognised = new(
        "Awards.Nomination.NotRecognised",
        "Could not identify award type being nominated");

    public static readonly Func<AwardNominationId, Error> NotFound = id => new(
        "Awards.Nomination.NotFound",
        $"Could not find a nomination with the id {id}");

    public static readonly Error InvalidCourseId = new(
        "Awards.Nomination.InvalidCourseId",
        "Could not identify the Course from the Course Id provided");

    public static readonly Error InvalidOfferingId = new(
        "Awards.Nomination.InvalidOfferingId",
        "Could not identify the Offering from the Offering Id provided");

    public static readonly Error DuplicateFound = new(
        "Awards.Nomination.DuplicateFound",
        "A nomination for this award and student already exists in the period");
}