namespace Constellation.Core.Models.Offerings.Errors;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using System;

public static class OfferingErrors
{
    public static readonly Func<int, string, Error> SearchFailed = (year, course) => new(
        "Offerings.Offering.SearchFailed",
        $"Could not identify Offering from Year {year} and name {course}.");

    public static readonly Func<OfferingId, Error> NotFound = id => new(
        "Offerings.Offering.NotFound",
        $"Could not find Offering with Id {id}");

    public static class Validation
    {
        public static readonly Error StartDateAfterEndDate = new(
            "Offerings.Offering.Validation.StartDate",
            "Start Date cannot be after the End Date");

        public static readonly Error EndDateInPast = new(
            "Offerings.Offering.Validation.EndDate",
            "End Date cannot be in the past");
    }
}
