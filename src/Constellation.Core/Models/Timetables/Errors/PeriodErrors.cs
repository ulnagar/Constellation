namespace Constellation.Core.Models.Timetables.Errors;

using Identifiers;
using Shared;
using System;

public static class PeriodErrors
{
    public static readonly Func<PeriodId, Error> NotFound = id => new(
        "Timetables.Period.NotFound",
        $"Could not find a Period with the Id {id}");

    public static readonly Error NoneFoundForOffering = new(
        "Timetables.Period.NoneFoundForOffering",
        "Could not find Periods attached to Offering");

    public static readonly Error InvalidId = new(
        "Timetables.Period.InvalidId",
        "An invalid Id was provided");

    public static readonly Error IsNull = new(
        "Timetables.Period.IsNull",
        "A Period is required, but none supplied");
}
