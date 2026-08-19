namespace Constellation.Core.Models.EnrolmentContext.Offer.Enums;

using Constellation.Core.Common;

public sealed class OfferStatus : StringEnumeration<OfferStatus>
{
    public static readonly OfferStatus Preparing = new("Preparing", 1);                        // before sending to parents
    public static readonly OfferStatus AwaitingResponse = new("Awaiting Response", 2);         // waiting for parent response
    public static readonly OfferStatus CollectingDocuments = new("Collecting Documents", 3);   // waiting for front office to collect court orders or health conditions
    public static readonly OfferStatus ReviewingResponse = new("Reviewing Response", 4);       // waiting for HTW to review documents and make recommendation
    public static readonly OfferStatus PendingAcceptance = new("Pending Acceptance", 5);       // waiting for Principal to give final approval
    public static readonly OfferStatus Accepted = new("Accepted", 6);                          // Principal approved, enrolment accepted
    public static readonly OfferStatus Declined = new("Declined", 7);                          // Principal or parent declined, enrolment rejected
    public static readonly OfferStatus Lapsed = new ("Lapsed", 8);                             // No response from parent, enrolment lapsed

    public OfferStatus(string value, int order)
    : base(value, value, order) { }

    public static IEnumerable<OfferStatus> GetOptions() 
        => GetEnumerable;
}