using Constellation.Core.Models.EnrolmentContext.Offer.Enums;

namespace Constellation.Core.Models.EnrolmentContext.Offer.Extensions;

public static class OfferStatusExtensions
{
    public static string GetColour(this OfferStatus status) => status switch
    {
        //_ when status == OfferStatus.Preparing => "#c9ad99",
        //_ when status == OfferStatus.AwaitingResponse => "#3e9896",
        //_ when status == OfferStatus.CollectingDocuments => "#d6828c",
        //_ when status == OfferStatus.ReviewingResponse => "#987143",
        //_ when status == OfferStatus.PendingAcceptance => "#233341",
        //_ when status == OfferStatus.Accepted => "#6098b6",
        //_ when status == OfferStatus.Declined => "#b60e10",
        //_ when status == OfferStatus.Lapsed => "#d1e2f0",
        _ when status == OfferStatus.Preparing => "#adb5bd",
        _ when status == OfferStatus.AwaitingResponse => "#6c757d",
        _ when status == OfferStatus.CollectingDocuments => "#ffc107",
        _ when status == OfferStatus.ReviewingResponse => "#dc3545",
        _ when status == OfferStatus.PendingAcceptance => "#198754",
        _ when status == OfferStatus.Accepted => "#003a7d",
        _ when status == OfferStatus.Declined => "#d83034",
        _ when status == OfferStatus.Lapsed => "#c701ff",
        _ => ""
    };
}