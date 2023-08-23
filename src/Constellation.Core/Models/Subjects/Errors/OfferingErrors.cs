namespace Constellation.Core.Models.Subjects.Errors;

using Constellation.Core.Shared;
using System;

public static class OfferingErrors
{
    public static readonly Func<int, string, Error> SearchFailed = (year, course) => new(
        "Subjects.Offering.SearchFailed",
        $"Could not identify Offering from Year {year} and name {course}.");
}