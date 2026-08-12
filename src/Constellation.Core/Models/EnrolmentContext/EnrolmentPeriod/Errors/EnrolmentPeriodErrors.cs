namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;

using Identifiers;
using Shared;

public static class EnrolmentPeriodErrors
{
    public static readonly Func<EnrolmentPeriodId, Error> NotFound = id => new(
        "Enrolment.Period.NotFound",
        $"Could not find an Enrolment Period with the id '{id}'");

    public static readonly Error InvalidId = new(
        "Enrolment.Period.InvalidId",
        "The provided Id is invalid");

    public static readonly Error OpenAtRequired = new(
        "Enrolment.Period.OpenAtRequired",
        "An Enrolment Period must have an opening time");

    public static readonly Error ClosedAtRequired = new(
        "Enrolment.Period.ClosedAtRequired",
        "An Enrolment Period must have a closing time");

    public static readonly Error CloseBeforeOpen = new(
        "Enrolment.Period.CloseBeforeOpen",
        "The provided ClosedAt time is before the OpenAt time");

    public static readonly Error OpenTooFarInPast = new(
        "Enrolment.Period.OpenTooFarInPast",
        "The provided OpenAt time is too far in the past to be valid");

    public static readonly Error CloseTooFarInFuture = new(
        "Enrolment.Period.CloseTooFarInFuture",
        "The provided ClosedAt time is too far in the future to be valid");

    public static readonly Error DurationTooShort = new(
        "Enrolment.Period.DurationTooShort",
        "The provided OpenAt and ClosedAt values are too close together");

    public static readonly Error DurationTooLong = new(
        "Enrolment.Period.DurationTooLong",
        "The provided OpenAt and ClosedAt values are too far apart");

    public static readonly Error CannotSuspendArchivedPeriod = new(
        "Enrolment.Period.CannotSuspendArchivedPeriod",
        "The current status is Archived, so you cannot suspend it");

    public static readonly Error PeriodMismatch = new(
        "Enrolment.Period.PeriodMismatch",
        "The requested record does not belong to the currently selected Enrolment Period");
}
