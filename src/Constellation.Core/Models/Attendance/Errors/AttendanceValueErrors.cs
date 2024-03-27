namespace Constellation.Core.Models.Attendance.Errors;

using Identifiers;
using Shared;
using System;

public static class AttendanceValueErrors
{
    public static Func<AttendanceValueId, Error> NotFound = id => new(
        "AttendanceValue.NotFound",
        $"Could not find an Attendance Value with the Id {id}");

    public static class Create
    {
        public static Error EmptyValues => new(
            "AttendanceValue.Create.EmptyValues",
            "At least one data value must not be zero");

        public static Error MinimumDates => new(
            "AttendanceValue.Create.MinimumDates",
            "Start Date and End Date must be valid dates");

        public static Error DateRangeInvalid => new(
            "AttendanceValue.Create.DateRangeInvalid",
            "The Start Date must be less than the End Date");
    }
}
