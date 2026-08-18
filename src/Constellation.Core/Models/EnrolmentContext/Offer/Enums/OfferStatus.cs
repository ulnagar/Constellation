namespace Constellation.Core.Models.EnrolmentContext.Offer.Enums;

using Constellation.Core.Common;

public sealed class OfferStatus : StringEnumeration<OfferStatus>
{
    public static readonly OfferStatus Preparing = new("Preparing");
    public static readonly OfferStatus AwaitingResponse = new("Awaiting Response");
    public static readonly OfferStatus CollectingDocuments = new("Collecting Documents");
    public static readonly OfferStatus ReviewingResponse = new("Reviewing Response");
    public static readonly OfferStatus PendingAcceptance = new("Pending Acceptance");
    public static readonly OfferStatus Accepted = new("Accepted");
    public static readonly OfferStatus Declined = new("Declined");
    public static readonly OfferStatus Lapsed = new ("Lapsed");

    public OfferStatus(string value)
    : base(value, value) { }
}