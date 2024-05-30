namespace Constellation.Core.Models.Attendance.Errors;

using Identifiers;
using Shared;
using System;

public static class AttendanceValueErrors
{
    public static readonly Func<AttendanceValueId, Error> NotFound = id => new(
        "AttendanceValue.NotFound",
        $"Could not find an Attendance Value with the Id {id}");

    public static readonly Func<string, Error> NotFoundForStudent = id => new(
        "AttendanceValue.NotFoundForStudent",
        $"Could not find an Attendance Value for the student with Id {id}");

    public static readonly Error CreateEmptyValues = new(
        "AttendanceValue.Create.EmptyValues",
        "At least one data value must not be zero");

    public static readonly Error CreateMinimumDates = new(
        "AttendanceValue.Create.MinimumDates",
        "Start Date and End Date must be valid dates");

    public static readonly Error CreateDateRangeInvalid = new(
        "AttendanceValue.Create.DateRangeInvalid",
        "The Start Date must be less than the End Date");
}
