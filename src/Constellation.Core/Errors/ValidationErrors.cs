namespace Constellation.Core.Errors;

using Constellation.Core.Shared;
using System;

public static class ValidationErrors
{
    public static class Date
    {
        public static readonly Func<DateOnly, DateOnly, Error> RangeReversed = (startDate, endDate) => new(
            "ValidationError.DateOnly.RangeReversed",
            $"A date range of {startDate} to {endDate} is invalid");

        public static readonly Func<DateOnly, DateOnly, DateOnly, Error> OutOfRange = (cancelDate, startDate, endDate) => new(
            "ValidationError.DateOnly.OutOfRange",
            $"The specified date ({cancelDate}) does not fall between {startDate} and {endDate}");
    }

    public static class String
    {
        public static readonly Func<string, Error> RequiredIsNull = variable => new(
            "ValidationError.String.RequiredIsNull",
            $"The required variable {variable} is null or empty");
    }
}
