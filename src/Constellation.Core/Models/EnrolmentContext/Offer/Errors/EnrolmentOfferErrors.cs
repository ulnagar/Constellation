namespace Constellation.Core.Models.EnrolmentContext.Offer.Errors;

using Enums;
using Identifiers;
using Shared;

public static class EnrolmentOfferErrors
{
    public static readonly Error InvalidId = new(
        "Enrolment.Offer.InvalidId",
        "The provided Id is invalid");

    public static readonly Error ApplicationNotApproved = new(
        "Enrolments.Offer.ApplicationNotApproved",
        "Cannot create an Offer from an Application that is not approved first");

    public static readonly Error NoneFound = new(
        "Enrolment.Offer.NoneFound",
        "No matching Enrolment Offers could be found");

    public static readonly Func<OfferStatus, OfferStatus, Error> InvalidStatusChange = (currentStatus, newStatus) => new(
        "Enrolment.Offer.InvalidStatusChange",
        $"Cannot change an Offer from {currentStatus} to {newStatus}");

    public static readonly Error ReminderInvalid = new(
        "Enrolment.Offer.ReminderInvalid",
        "A reminder is not required for an Offer that is not Pending");

    public static readonly Func<OfferId, Error> NotFound = id => new(
        "Enrolment.Offer.NotFound",
        $"Could not find an Enrolment Offer with the Id '{id}'");
}