namespace Constellation.Core.Models.Students.Errors;

using Shared;
using System;

public static class AbsenceConfigurationErrors
{
    public static readonly Error AlreadyCancelled = new(
        "Student.AbsenceConfiguration.AlreadyCancelled",
        "This absence configuration has already been marked cancelled");

    public static readonly Func<DateOnly, DateOnly, Error> RecordForRangeExists = (startDate, endDate) => new(
        "Student.AbsenceConfiguration.RecordForRangeExists",
        $"A current configuration exists that covers some or all of the dates from {startDate} to {endDate}");

}
