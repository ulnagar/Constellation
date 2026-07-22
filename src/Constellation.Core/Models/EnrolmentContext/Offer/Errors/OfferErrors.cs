namespace Constellation.Core.Models.EnrolmentContext.Offer.Errors;

using Shared;

public static class OfferErrors
{
    public static readonly Error ApplicationNotApproved = new(
        "Enrolments.Offer.ApplicationNotApproved",
        "Cannot create an Offer from an Application that is not approved first");

}
